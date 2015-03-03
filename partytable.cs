using System;
using System.Diagnostics;
using System.Net;

namespace KeySigningPartyList
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: partytable.exe [pub keyring]");
                return;
            }

            string keyInfoStr = GetKeyList(args[0]);

            bool havePubKey = false;
            bool haveUidForPubKey = false;
            bool haveFprForPubKey = false;

            string keyId = "";
            string fpr = "";
            string keyType = "";
            string keyLength = "";
            string uid = "";

            Console.WriteLine("<!DOCTYPE html>\n");
            Console.WriteLine("<html>\n");
            Console.WriteLine("<meta charset=\"utf-8\" />\n");
            Console.WriteLine("<head>\n");
            Console.WriteLine("<style type=\"text/css\">");
            Console.WriteLine(@"
body { font-size: 1em; }
table { border-width: 1px; border-style: solid; border-color: black; border-collapse: collapse; }
table th { border: 1px solid black; padding: 2px; font-size: 1.2em; }
table td { border: 1px solid black; padding: 4px; font-size: 1.2em; }
table td pre { margin: 0; }");
            Console.WriteLine("</style>");
            Console.WriteLine("</head>\n");
            Console.WriteLine("<body>\n");
            Console.WriteLine("<h1>Keysigning Party</h1>\n");
            Console.WriteLine("<table><tr><th>#</th><th>Key ID</th><th>Owner</th><th>Fingerprint</th><th>Size</th><th>Type</th><th>Key Info<br />Matches?</th><th>Owner ID<br />Matches?</th></tr>\n");
            int count = 1;
            foreach (var line in keyInfoStr.Split('\n'))
            {
                var parts = line.Split(':');
                if (!havePubKey && !haveUidForPubKey && !haveFprForPubKey)
                {
                    if (parts[0] == "pub")
                    {
                        havePubKey = true;
                        keyType = GetKeyType(parts[3]);
                        keyLength = parts[2];
                        keyId = parts[4].Substring(parts[4].Length - 8);
                    }
                }

                if (havePubKey && !haveUidForPubKey && !haveFprForPubKey)
                {
                    if (parts[0] == "fpr")
                    {
                        haveFprForPubKey = true;
                        fpr = parts[9];
                    }
                }

                if (havePubKey && haveFprForPubKey && !haveUidForPubKey)
                {
                    if (parts[0] == "uid")
                    {
                        haveUidForPubKey = true;
                        uid = parts[9];
                    }
                }

                if (havePubKey && haveUidForPubKey && haveFprForPubKey)
                {
                    Console.WriteLine("\t<tr><td>{5}</td><td><pre>{0}</pre></td><td>{1}</td><td><pre>{2}</pre></td><td>{3}</td><td>{4}</td><td>&nbsp;</td><td>&nbsp;</td></tr>\n", keyId, WebUtility.HtmlEncode(uid), SplitFingerPrint(fpr), keyLength, keyType, count);
                    havePubKey = false;
                    haveUidForPubKey = false;
                    haveFprForPubKey = false;
                    count++;
                }
            }
            Console.WriteLine("</table>\n");
            Console.WriteLine("<small>List generated: {0}</small>", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.WriteLine("</body>\n");
            Console.WriteLine("</html>\n");
        }

        public static string GetKeyType(string type)
        {
            switch(type) {
                case "1":
                    return "RSA";
                case "17":
                    return "DSA";
                case "20":
                    return "El Gamal";
                case "22":
                    return "ECDSA";
                default:
                    return "Unknown";
            }
        }

        private static string SplitFingerPrint(string fpr) 
        {
            string prettyFpr = "";
            for(int i = 0; i < fpr.Length; i += 4) 
            {
                prettyFpr += fpr.Substring(i, 4) + " ";
                if(i == 16) 
                {
                    prettyFpr += "\n";
                }
            }
            return prettyFpr;
        }

        private static string GetKeyList(string keyring) 
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "gpg2";
            p.StartInfo.Arguments = "--fingerprint --no-default-keyring --no-options --with-colons --keyring " + keyring;
            p.Start();

            string keyInfoStr = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return keyInfoStr;
        }
    }
}
