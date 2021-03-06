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

using Microsoft.Azure.Commands.KeyVault.Models;
using Microsoft.Azure.KeyVault.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.KeyVault
{
    /// <summary>
    /// Removes a given certificate contact from Key Vault for certificate management
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, CmdletNoun.AzureKeyVaultCertificateContact,
        SupportsShouldProcess = true,
        HelpUri = Constants.KeyVaultHelpUri)]
    [OutputType(typeof(List<PSKeyVaultCertificateContact>))]
    public class RemoveAzureKeyVaultCertificateContact : KeyVaultCmdletBase
    {
        /// <summary>
        /// VaultName
        /// </summary>
        [Parameter(Mandatory = true,
                   Position = 0,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Vault name. Cmdlet constructs the FQDN of a vault based on the name and currently selected environment.")]
        [ValidateNotNullOrEmpty]
        public string VaultName { get; set; }

        /// <summary>
        /// EmailAddress
        /// </summary>
        [Parameter(Mandatory = true,
                   Position = 1,
                   ValueFromPipelineByPropertyName = true,
                   HelpMessage = "Specifies the email address of the contact.")]
        [ValidateNotNullOrEmpty]
        public string[] EmailAddress { get; set; }

        /// <summary>
        /// PassThru parameter
        /// </summary>
        [Parameter(Mandatory = false,
                   HelpMessage = "This cmdlet does not return an object by default. If this switch is specified, it returns the contact object.")]
        public SwitchParameter PassThru { get; set; }

        public override void ExecuteCmdlet()
        {
            if (ShouldProcess(VaultName, Properties.Resources.RemoveCertificateContact))
            {
                Contacts existingContacts;

                try
                {
                    existingContacts = this.DataServiceClient.GetCertificateContacts(VaultName);
                }
                catch (KeyVaultErrorException exception)
                {
                    if (exception.Response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        throw;
                    }

                    existingContacts = null;
                }

                List<Contact> existingContactList;

                if (existingContacts == null ||
                    existingContacts.ContactList == null)
                {
                    existingContactList = new List<Contact>();
                }
                else
                {
                    existingContactList = new List<Contact>(existingContacts.ContactList);
                }

                foreach (var email in EmailAddress)
                {
                    existingContactList.RemoveAll(contact => string.Compare(contact.EmailAddress, email, StringComparison.OrdinalIgnoreCase) == 0);
                }

                if (existingContactList.Count == 0)
                {
                    existingContactList = null;
                }

                var resultantContacts = this.DataServiceClient.SetCertificateContacts(VaultName, new Contacts { ContactList = existingContactList });

                if (PassThru.IsPresent)
                {
                    this.WriteObject(PSKeyVaultCertificateContact.FromKVCertificateContacts(resultantContacts, VaultName));
                }
            }
        }
    }
}
