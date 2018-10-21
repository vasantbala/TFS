using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSChangeHistory
{
    public class ChangesetManager
    {
        public static List<ChangesetViewModel> GetChangesetHistory(ChangesetHistoryRequest request)
        { 
            List<ChangesetViewModel> changesetList = new List<ChangesetViewModel>();

            using (TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(new Uri(request.TFSUrl)))
            {
                tpc.EnsureAuthenticated();
                VersionControlServer vcs = tpc.GetService<VersionControlServer>();

                VersionSpec fromDateVersion = new DateVersionSpec(request.FromDate.AddDays(-1));
                VersionSpec toDateVersion = new DateVersionSpec(request.ToDate);

                IEnumerable changesets = vcs.QueryHistory(request.ReleaseBranchUrl, VersionSpec.Latest,
                    0, RecursionType.Full, null, fromDateVersion, toDateVersion, int.MaxValue, true, true);

                var ignoreUsers = request.IgnoreFromUsers;

                foreach (Changeset changeset in changesets)
                {
                    if (ignoreUsers.Contains(changeset.Owner) == false)
                    {
                        changesetList.Add(new ChangesetViewModel() { ChangesetId = changeset.ChangesetId, Owner = changeset.Owner, Comment = changeset.Comment, CheckInDateTime = changeset.CreationDate });
                    }
                }
            }
            return changesetList;
        }

        private static void ValidateRequest(ChangesetHistoryRequest request)
        {
            bool result = Uri.TryCreate(request.TFSUrl, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                throw new ArgumentException("TFS URL is not valid");
            }

            if (string.IsNullOrEmpty(request.ReleaseBranchUrl))
            {
                throw new ArgumentNullException("Release Branch Url cannot be null");
            }
        }
    }
}
