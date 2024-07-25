/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago {
    public enum LicenseLogModes {
        Off = 0,
        FirstParty = 1,
        All = 3,
    }

    public class License {
        public const string PLUGIN_LICENSE =
@"
    Copyright 2024 JKLeckr

    Licensed under the Apache License, Version 2.0 (the ""License"");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License located in LICENSE.txt or at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an ""AS IS"" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.";

        public const string PLUGIN_LIB_LICENSE = 
@"
    This mod uses the following third party libraries:

     Libraries licensed under the MIT License:
    
         Archipelago.MultiClient.NET
         Copyright (c) 2022 Hussein Farran, Jarno Westhof

         Archipelago.MultiClient.NET also includes (also MIT Licensed):
             Newtonsoft Json.NET
             Copyright (c) 2007 James Newton-King

             websocket-sharp
             Copyright (c) 2010-2024 sta.blockhead
    
     You may obtain a copy of the MIT license in LICENSE-3RD-PARTY.txt
     or at https://mit-license.org/";

        public const string PLUGIN_LIB_NOTICE = 
@"
    This mod uses third party libraries.
    To see their notices, set ""LogLicense"" to ""All"" in the config.";
    }
}
