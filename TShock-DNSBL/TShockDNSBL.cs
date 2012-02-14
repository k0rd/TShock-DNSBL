using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using Hooks;
using TShockAPI;
using Terraria;

namespace TShock_DNSBL
{
    [APIVersion(1, 11)]
    public class Dnsbl : TerrariaPlugin
    {
        internal static string WhitelistPath
        {
            get { return Path.Combine(TShock.SavePath, "whitelist.txt"); }
        }
        internal static string BlListPath
        {
            get { return Path.Combine(TShock.SavePath, "dnsbl.server"); }
        }
        public Dnsbl(Main game)
            : base(game)
        {

            Order = 4;
  

        }

        public override void Initialize()
        {
            ServerHooks.Connect += OnConnect;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerHooks.Connect -=OnConnect;
            }
            base.Dispose(disposing);
        }

        public override Version Version
        {
            get { return new Version("1.0"); }
        }

        public override string Name
        {
            get { return "TShock DNS Blacklist system"; }
        }

        public override string Author
        {
            get { return "by k0rd"; }
        }

        public override string Description
        {
            get { return "Proxy blocking using a DNSBL provider"; }
        }

        public void OnConnect(int pID, HandledEventArgs e)
        {
            string[] blProvider = new string[] {GetDNSBLServer()};

           CheckProxy m_checker= new CheckProxy(TShock.Players[pID].IP,blProvider);
        
            if (m_checker.IPAddr.Valid)
            {
                if (m_checker.BlackList.IsListed)
                {
                    TShock.Utils.ForceKick(TShock.Players[pID], string.Format("You are listed on the blacklist at {0}.", m_checker.BlackList.VerifiedOnServer));
                    e.Handled = true;
                    return;
                }
            }


        }

        public static string GetDNSBLServer()
        {
            FileTools.CreateIfNot(BlListPath, "xbl.spamhaus.org");
            using (var tr = new StreamReader(BlListPath))
            {
                string bllist = tr.ReadLine();
                return bllist;
            }
        }

        public static bool CheckWhitelist(string ip)
        {
            FileTools.CreateIfNot(WhitelistPath, "127.0.0.1");
            using (var tr = new StreamReader(WhitelistPath))
            {
                string whitelist = tr.ReadToEnd();
                ip = TShock.Utils.GetRealIP(ip);
                bool contains = whitelist.Contains(ip);
                if (!contains)
                {
                    foreach (var line in whitelist.Split(Environment.NewLine.ToCharArray()))
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                        contains = TShock.Utils.GetIPv4Address(line).Equals(ip);
                        if (contains)
                            return true;
                    }
                    return false;
                }
                return true;
            }
        }


        public class CheckProxy
        {
            #region Nested classes
            public class RrIpAddress
            {
                #region Private fields

                private string[] _adre;
                private bool _valid;

                #endregion

                #region Class Properties

                public bool Valid
                {
                    get { return _valid; }
                }

                public string AsString
                {
                    get
                    {
                        if (_valid)
                        {
                            string tmpstr = "";
                            for (int ai = 0; ai < _adre.Length; ai++)
                            {
                                tmpstr += _adre[ai];
                                if (ai < _adre.Length - 1)
                                    tmpstr += ".";
                            }
                            return tmpstr;
                        }
                        else
                            return "";
                    }
                    set
                    {
                        _adre = value.Split(new Char[] { '.' });
                        if (_adre.Length == 4)
                        {
                            try
                            {
                                _valid = true;
                                byte tmpx = 0;
                                foreach (string addsec in _adre)
                                    if (!byte.TryParse(addsec, out tmpx))
                                    {
                                        _valid = false;
                                        break;
                                    }
                            }
                            catch { _valid = false; }
                        }
                        else
                            _valid = false;
                    }
                }

                public string AsRevString
                {
                    get
                    {
                        if (_valid)
                        {
                            string tmpstr = "";
                            for (int ai = _adre.Length - 1; ai >= 0; ai--)
                            {
                                tmpstr += _adre[ai];
                                if (ai > 0)
                                    tmpstr += ".";
                            }
                            return tmpstr;
                        }
                        else
                            return "";
                    }
                }

                public string[] AsStringArray
                {
                    get { return _adre; }
                    set
                    {
                        if (value.Length == 4)
                        {
                            try
                            {
                                _valid = true;
                                byte tmpx = 0;
                                foreach (string addsec in value)
                                    if (!byte.TryParse(addsec, out tmpx))
                                    {
                                        _valid = false;
                                        break;
                                    }
                            }
                            catch { _valid = false; }
                        }
                        else
                            _valid = false;
                    }
                }

                public string[] AsRevStringArray
                {
                    get
                    {
                        string[] tmpstrarr = new string[_adre.Length];
                        for (int ai = _adre.Length - 1; ai >= 0; ai--)
                        {
                            tmpstrarr[(_adre.Length - 1) - ai] = _adre[ai];
                        }
                        return tmpstrarr;
                    }
                }

                public byte[] AsByteArray
                {
                    get
                    {
                        if (_valid)
                            return StringToByte(_adre);
                        else
                            return new byte[0];
                    }
                    set
                    {
                        if (value.Length == 4)
                        {
                            _adre = ByteToString(value);
                            _valid = true;
                        }
                        else
                            _valid = false;
                    }
                }

                public byte[] AsRevByteArray
                {
                    get
                    {
                        byte[] tmpcon = StringToByte(_adre);
                        byte[] tmpbytearr = new byte[tmpcon.Length];
                        for (int ai = tmpcon.Length - 1; ai >= 0; ai--)
                        {
                            tmpbytearr[(tmpcon.Length - 1) - ai] = tmpcon[ai];
                        }
                        return tmpbytearr;
                    }
                }

                public long AsLong
                {
                    get
                    {
                        if (_valid)
                            return StringToLong(_adre, true);
                        else
                            return 0;
                    }
                    set
                    {
                        try
                        {
                            _adre = LongToString(value, false);
                            _valid = true;
                        }
                        catch { _valid = false; }
                    }
                }

                public long AsRevLong
                {
                    get { return StringToLong(_adre, false); }
                }

                #endregion

                #region Contructors

                public RrIpAddress() { }
                public RrIpAddress(string address)
                {
                    this.AsString = address;
                }
                public RrIpAddress(string[] address)
                {
                    this.AsStringArray = address;
                }
                public RrIpAddress(byte[] address)
                {
                    this.AsByteArray = address;
                }
                public RrIpAddress(long address)
                {
                    this.AsLong = address;
                }

                #endregion

                #region Private methods

                private byte[] StringToByte(string[] strArray)
                {
                    try
                    {
                        byte[] tmp = new byte[strArray.Length];
                        for (int ia = 0; ia < strArray.Length; ia++)
                            tmp[ia] = byte.Parse(strArray[ia]);
                        return tmp;
                    }
                    catch
                    {
                        return new byte[0];
                    }
                }

                private string[] ByteToString(byte[] byteArray)
                {
                    try
                    {
                        string[] tmp = new string[byteArray.Length];
                        for (int ia = 0; ia < byteArray.Length; ia++)
                            tmp[ia] = byteArray[ia].ToString();
                        return tmp;
                    }
                    catch
                    {
                        return new string[0];
                    }
                }

                private long StringToLong(string[] straddr, bool m_bbck)
                {
                    long num = 0;
                    if (straddr.Length == 4)
                    {
                        try
                        {
                            if (m_bbck)
                                num = (int.Parse(straddr[0])) +
                                    (int.Parse(straddr[1]) * 256) +
                                    (int.Parse(straddr[2]) * 65536) +
                                    (int.Parse(straddr[3]) * 16777216);
                            else
                                num = (int.Parse(straddr[3])) +
                                    (int.Parse(straddr[2]) * 256) +
                                    (int.Parse(straddr[1]) * 65536) +
                                    (int.Parse(straddr[0]) * 16777216);
                        }
                        catch { num = 0; }
                    }
                    else
                        num = 0;
                    return num;
                }

                private string[] LongToString(long lngval, bool Revese)
                {
                    string[] tmpstrarr = new string[4];
                    if (lngval > 0)
                    {
                        try
                        {
                            int a = (int)(lngval / 16777216) % 256;
                            int b = (int)(lngval / 65536) % 256;
                            int c = (int)(lngval / 256) % 256;
                            int d = (int)(lngval) % 256;
                            if (Revese)
                            {
                                tmpstrarr[0] = a.ToString();
                                tmpstrarr[1] = b.ToString();
                                tmpstrarr[2] = c.ToString();
                                tmpstrarr[3] = d.ToString();
                            }
                            else
                            {
                                tmpstrarr[3] = a.ToString();
                                tmpstrarr[2] = b.ToString();
                                tmpstrarr[1] = c.ToString();
                                tmpstrarr[0] = d.ToString();
                            }
                        }
                        catch { }
                        return tmpstrarr;
                    }
                    else
                        return tmpstrarr;
                }

                #endregion
            }

            public class BlackListed
            {
                #region private fields

                private bool _IsListed;
                private string _verifiedonserver;

                #endregion

                #region Class properties

                public string VerifiedOnServer
                {
                    get { return _verifiedonserver; }
                }

                public bool IsListed
                {
                    get { return _IsListed; }
                }

                #endregion

                #region Contructor

                public BlackListed(bool listed, string server)
                {
                    this._IsListed = listed;
                    this._verifiedonserver = server;
                }

                #endregion
            }

            #endregion

            #region Private fields

            private RrIpAddress _ip;
            private BlackListed _blacklisted = new BlackListed(false, "");

            #endregion

            #region Class Properties

            public RrIpAddress IPAddr
            {
                get { return _ip; }
                set { _ip = value; }
            }

            public BlackListed BlackList
            {
                get { return _blacklisted; }
            }

            #endregion

            #region Constructors

            public CheckProxy(byte[] address, string[] blacklistservers)
            {
                _ip = new RrIpAddress(address);
                VerifyOnServers(blacklistservers);
            }
            public CheckProxy(long address, string[] blacklistservers)
            {
                _ip = new RrIpAddress(address);
                VerifyOnServers(blacklistservers);
            }
            public CheckProxy(string address, string[] blacklistservers)
            {
                _ip = new RrIpAddress(address);
                VerifyOnServers(blacklistservers);
            }
            public CheckProxy(RrIpAddress address, string[] blacklistservers)
            {
                _ip = address;
                VerifyOnServers(blacklistservers);
            }

            #endregion

            #region Private methods

            private void VerifyOnServers(string[] _blacklistservers)
            {
                _blacklisted = null;
                if (_blacklistservers != null && _blacklistservers.Length > 0)
                {
                    foreach (string BLSrv in _blacklistservers)
                    {
                        if (VerifyOnServer(BLSrv))
                        {
                            _blacklisted = new BlackListed(true, BLSrv);
                            break;
                        }
                    }
                    if (_blacklisted == null)
                        _blacklisted = new BlackListed(false, "");
                }
            }

            private bool VerifyOnServer(string BLServer)
            {
                if (_ip.Valid)  //If IP address is valid continue..
                {
                    try
                    {
                        IPHostEntry ipEntry = Dns.GetHostEntry(_ip.AsRevString + "." +
                    BLServer);  //Look up the IP address on the BLServer
                        ipEntry = null; //Clear the object
                        return true; //IP address was found on the BLServer,
                        //it's then listed in the black list
                    }
                    catch (System.Net.Sockets.SocketException dnserr)
                    {
                        if (dnserr.ErrorCode == 11001) // IP address not listed
                            return false;
                        else // Another error happened - ignore?
                            return false;
                    }
                }
                else
                    return false;   //IP address is not valid
            }

            #endregion
        }

    }
}