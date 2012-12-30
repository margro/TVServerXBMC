/*
 *	Copyright (C) 2007-2012 ARGUS TV
 *	http://www.argus-tv.com
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace TVServerXBMC.Common
{
    public class PlatformInfo
    {
        /// <summary>
        /// Is this an NT platform?
        /// </summary>
        public static bool IsWindowsNT
        {
            get 
            { 
                return (PlatformID.Win32NT == Environment.OSVersion.Platform); 
            }
        }

        /// <summary>
        /// Returns true if this is Windows 2000 or higher
        /// </summary>
        public static bool IsWindows2KUp
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;
                return (PlatformID.Win32NT == os.Platform && os.Version.Major >= 5);
            }
        }
    }
}
