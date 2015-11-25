﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Management.Automation;
using Microsoft.Azure.Management.SiteRecovery.Models;
using Microsoft.Azure.Portal.RecoveryServices.Models.Common;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Properties = Microsoft.Azure.Commands.SiteRecovery.Properties;

namespace Microsoft.Azure.Commands.SiteRecovery
{
    /// <summary>
    /// Creates Azure Site Recovery Policy object in memory.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmSiteRecoverySite", DefaultParameterSetName = ASRParameterSets.Default)]
    public class RemoveAzureSiteRecoverySite : SiteRecoveryCmdletBase
    {
        #region Parameters
       
        /// <summary>
        /// Gets or sets the site name
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRSite Site { get; set; }

        #endregion

        /// <summary>
        /// ProcessRecord of the command.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                RecoveryServicesProviderListResponse recoveryServicesProviderListResponse =
                        RecoveryServicesClient.GetAzureSiteRecoveryProvider(
                        this.Site.Name);

                if (recoveryServicesProviderListResponse.RecoveryServicesProviders.Count != 0)
                {
                    throw new PSInvalidOperationException(Properties.Resources.SiteRemovalWithRegisteredHyperVHostsError);
                }

                FabricDeletionInput input = new FabricDeletionInput()
                {
                    Properties = new FabricDeletionInputProperties()
                };

                LongRunningOperationResponse response =
                 RecoveryServicesClient.DeleteAzureSiteRecoveryFabric(this.Site.Name, input);

                JobResponse jobResponse =
                    RecoveryServicesClient
                    .GetAzureSiteRecoveryJobDetails(PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

                WriteObject(new ASRJob(jobResponse.Job));
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
        }
    }
}