/*
 *	Copyright (C) 2010-2012 Marcel Groothuis
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Management;
using System.Security.Principal;

using TvDatabase;

namespace TVServerKodi
{
  internal static class Utility
  {
    public static List<TvDatabase.Card> GetAllCards()
    {
      List<TvDatabase.Card> cards = new List<TvDatabase.Card>();
      IList<TvDatabase.Card> mediaPortalCards = TvDatabase.Card.ListAll();
      foreach (TvDatabase.Card card in mediaPortalCards)
      {
        if (card.Enabled
            && !card.DevicePath.Equals("(builtin)", StringComparison.CurrentCultureIgnoreCase))
        {
          cards.Add(card);
        }
      }
      return cards;
    }

    public static bool CreateUncShare(string shareName, string localPath)
    {
      ManagementScope scope = new System.Management.ManagementScope(@"root\CIMV2");
      scope.Connect();

      using (ManagementClass managementClass = new ManagementClass(scope, new ManagementPath("Win32_Share"), (ObjectGetOptions) null))
      {
        SecurityIdentifier securityIdentifier = new SecurityIdentifier(WellKnownSidType.WorldSid, (SecurityIdentifier) null);
        byte[] binaryForm = new byte[securityIdentifier.BinaryLength];
        securityIdentifier.GetBinaryForm(binaryForm, 0);

        using (ManagementObject wmiTrustee = new ManagementClass(scope, new ManagementPath("Win32_Trustee"), (ObjectGetOptions) null).CreateInstance())
        {
          wmiTrustee["SID"] = (object) binaryForm;
          using (ManagementObject wmiACE = new ManagementClass(scope, new ManagementPath("Win32_ACE"), (ObjectGetOptions) null).CreateInstance())
          {
            wmiACE["AccessMask"] = 131241; //READ_CONTROL | FILE_READ | FILE_TRAVERSE | FILE_READ_EA | FILE_LIST_DIRECTORY
            wmiACE["AceFlags"] = 3;        //OBJECT_INHERIT_ACE | CONTAINER_INHERIT_ACE
            wmiACE["AceType"] = 0; //ACCESS_ALLOWED
            wmiACE["Trustee"] = wmiTrustee;
            using (ManagementObject wmiSecurityDescriptor = new ManagementClass(scope, new ManagementPath("Win32_SecurityDescriptor"), (ObjectGetOptions) null).CreateInstance())
            { 
              wmiSecurityDescriptor["ControlFlags"] = 4;
              wmiSecurityDescriptor["DACL"] = new ManagementObject[] { wmiACE };
              using (ManagementBaseObject inParamsCreate = managementClass.GetMethodParameters("Create"))
              {
                inParamsCreate["Access"] = wmiSecurityDescriptor;
                inParamsCreate["Path"] = localPath;
                inParamsCreate["Name"] = shareName;
                inParamsCreate["Type"] = 0;
                inParamsCreate["Description"] = "TVServerXBMC share";
                using (ManagementBaseObject outParams = managementClass.InvokeMethod("Create", inParamsCreate, (InvokeMethodOptions) null))
                  return ((int) (uint) outParams["returnValue"] == 0);
              }
            }
          }
        }
      }
    }
  }
}