/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago {
  public enum LicenseLogModes {
      Off = 0,
      FirstParty = 1,
      All = 3,
  }

  public class ModLicense {
    public readonly string PLUGIN_NOTICE =
@"
    Copyright 2025 JKLeckr

    Licensed under the Apache License, Version 2.0 (the ""License"");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an ""AS IS"" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.";

    public readonly string PLUGIN_LIB_NOTICE =
@"
    This mod uses third party libraries.
    For their notices, see the accompanying LICENSE.third-party.txt or a copy at
    You can set ""LogLicense = All"" in the config to print the third party notice.";
  }

  public class ModLicenseThirdParty {
    public readonly string PLUGIN_LIB_FULL_NOTICE =
@"CupheadArchipelago uses third party libraries listed in this document.

FVer

    Copyright 2025 JKLeckr

    Licensed under the Apache License, Version 2.0 (the ""License"");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

        http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an ""AS IS"" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.


Archipelago.MultiClient.NET

    MIT License

    Copyright (c) 2022 Hussein Farran, Jarno Westhof

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the ""Software""), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

    Archipelago.MultiClient.NET also includes:
        
        A modified fork of Newtonsoft Json.NET

            The MIT License (MIT)

            Copyright (c) 2007 James Newton-King

            Permission is hereby granted, free of charge, to any person obtaining a copy
            of this software and associated documentation files (the ""Software""), to deal
            in the Software without restriction, including without limitation the rights
            to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
            copies of the Software, and to permit persons to whom the Software is
            furnished to do so, subject to the following conditions:

            The above copyright notice and this permission notice shall be included in all
            copies or substantial portions of the Software.
            
            THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
            OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
            SOFTWARE.


c-wspp websocket-sharp and c-wspp

    MIT License

    Copyright (c) 2023 black-sliver

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the ""Software""), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

    Binary distributions may contain parts of OpenSSL and other software.
    For OpenSSL see https://www.openssl.org/source/apache-license-2.0.txt
    See subprojects on https://github.com/black-sliver/c-wspp/
    for more details and individual licenses.
    Or read below.

    c-wspp also includes:

        ASIO

            Copyright (c) 2003-2021 Christopher M. Kohlhoff (chris at kohlhoff dot com)

            Distributed under the Boost Software License, Version 1.0. (See accompanying
            file LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)


        WebSocket++

            Main Library:

            Copyright (c) 2014, Peter Thorson. All rights reserved.

            Redistribution and use in source and binary forms, with or without
            modification, are permitted provided that the following conditions are met:
                * Redistributions of source code must retain the above copyright
                  notice, this list of conditions and the following disclaimer.
                * Redistributions in binary form must reproduce the above copyright
                  notice, this list of conditions and the following disclaimer in the
                  documentation and/or other materials provided with the distribution.
                * Neither the name of the WebSocket++ Project nor the
                  names of its contributors may be used to endorse or promote products
                  derived from this software without specific prior written permission.

            THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ""AS IS""
            AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
            IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
            ARE DISCLAIMED. IN NO EVENT SHALL PETER THORSON BE LIABLE FOR ANY
            DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
            (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
            LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
            ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
            (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
            SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

            Bundled Libraries:

            ****** Base 64 Library (base64/base64.hpp) ******
            base64.hpp is a repackaging of the base64.cpp and base64.h files into a
            single header suitable for use as a header only library. This conversion was
            done by Peter Thorson (webmaster@zaphoyd.com) in 2012. All modifications to
            the code are redistributed under the same license as the original, which is
            listed below.

            base64.cpp and base64.h

            Copyright (C) 2004-2008 René Nyffenegger

            This source code is provided 'as-is', without any express or implied
            warranty. In no event will the author be held liable for any damages
            arising from the use of this software.

            Permission is granted to anyone to use this software for any purpose,
            including commercial applications, and to alter it and redistribute it
            freely, subject to the following restrictions:

            1. The origin of this source code must not be misrepresented; you must not
              claim that you wrote the original source code. If you use this source code
              in a product, an acknowledgment in the product documentation would be
              appreciated but is not required.

            2. Altered source versions must be plainly marked as such, and must not be
              misrepresented as being the original source code.

            3. This notice may not be removed or altered from any source distribution.

            René Nyffenegger rene.nyffenegger@adp-gmbh.ch

            ****** SHA1 Library (sha1/sha1.hpp) ******
            sha1.hpp is a repackaging of the sha1.cpp and sha1.h files from the shallsha1
            library (http://code.google.com/p/smallsha1/) into a single header suitable for
            use as a header only library. This conversion was done by Peter Thorson
            (webmaster@zaphoyd.com) in 2013. All modifications to the code are redistributed
            under the same license as the original, which is listed below.

            Copyright (c) 2011, Micael Hildenborg
            All rights reserved.

            Redistribution and use in source and binary forms, with or without
            modification, are permitted provided that the following conditions are met:
                * Redistributions of source code must retain the above copyright
                  notice, this list of conditions and the following disclaimer.
                * Redistributions in binary form must reproduce the above copyright
                  notice, this list of conditions and the following disclaimer in the
                  documentation and/or other materials provided with the distribution.
                * Neither the name of Micael Hildenborg nor the
                  names of its contributors may be used to endorse or promote products
                  derived from this software without specific prior written permission.

            THIS SOFTWARE IS PROVIDED BY Micael Hildenborg ''AS IS'' AND ANY
            EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
            WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
            DISCLAIMED. IN NO EVENT SHALL Micael Hildenborg BE LIABLE FOR ANY
            DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
            (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
            LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
            ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
            (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
            SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

            ****** MD5 Library (common/md5.hpp) ******
            md5.hpp is a reformulation of the md5.h and md5.c code from
            http://www.opensource.apple.com/source/cups/cups-59/cups/md5.c to allow it to
            function as a component of a header only library. This conversion was done by
            Peter Thorson (webmaster@zaphoyd.com) in 2012 for the WebSocket++ project. The
            changes are released under the same license as the original (listed below)

            Copyright (C) 1999, 2002 Aladdin Enterprises.  All rights reserved.

            This software is provided 'as-is', without any express or implied
            warranty.  In no event will the authors be held liable for any damages
            arising from the use of this software.

            Permission is granted to anyone to use this software for any purpose,
            including commercial applications, and to alter it and redistribute it
            freely, subject to the following restrictions:

            1. The origin of this software must not be misrepresented; you must not
             claim that you wrote the original software. If you use this software
             in a product, an acknowledgment in the product documentation would be
             appreciated but is not required.
            2. Altered source versions must be plainly marked as such, and must not be
             misrepresented as being the original software.
            3. This notice may not be removed or altered from any source distribution.

            L. Peter Deutsch
            ghost@aladdin.com

            ****** UTF8 Validation logic (utf8_validation.hpp) ******
            utf8_validation.hpp is adapted from code originally written by Bjoern Hoehrmann
            <bjoern@hoehrmann.de>. See http://bjoern.hoehrmann.de/utf-8/decoder/dfa/ for
            details.

            The original license:

            Copyright (c) 2008-2009 Bjoern Hoehrmann <bjoern@hoehrmann.de>

            Permission is hereby granted, free of charge, to any person obtaining a copy
            of this software and associated documentation files (the ""Software""), to deal
            in the Software without restriction, including without limitation the rights
            to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
            copies of the Software, and to permit persons to whom the Software is
            furnished to do so, subject to the following conditions:

            The above copyright notice and this permission notice shall be included in
            all copies or substantial portions of the Software.

            THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
            OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
            SOFTWARE.
            
            
CupheadArchipelago partially uses code from BepInEx for writing its log files

    MIT License

    Copyright (c) 2018 Bepis

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the ""Software""), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.";
    }
}
