using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CJCOR
{
    class Program
    {
        class Node
        {
            public int l, r;
            public Node nxt, lst;
            public Node(int L, int R)
            {
                l = L;
                r = R;
                nxt = null;
                lst = null;
            }
        }
        static String filein, fileout;
        static int toint(int x)
        {
            if (x < 0)
            {
                x += 256;
            }
            return x;
        }
        static void fastrender(bool sound)
        {
            BufferedStream inp = new BufferedStream(File.Open(filein, FileMode.Open, FileAccess.Read, FileShare.Read), 1048576);
            BufferedStream otp = new BufferedStream(File.Open(fileout, FileMode.Create, FileAccess.Write, FileShare.Write), 1048576);
            Console.WriteLine("CJCOR Start! Please wait...");
            ArrayList[] hno = new ArrayList[4];
            for (int i = 0; i < 4; i++)
            {
                hno[i] = new ArrayList();
            }
            Node[] hds = new Node[256];
            for (int i = 0; i < 256; i++)
            {
                hds[i] = new Node(-1, -1);
            }
            int ReadByte()
            {
                int b = inp.ReadByte();
                if (b == -1) throw new Exception("Unexpected file end");
                return b;
            }
            for (int i = 0; i < 10; i++)
            {
                otp.WriteByte((byte)ReadByte());
            }
            int trkcnt;
            trkcnt = (toint(ReadByte()) * 256) + toint(ReadByte());
            otp.WriteByte((byte)(trkcnt / 256));
            otp.WriteByte((byte)(trkcnt % 256));
            otp.WriteByte((byte)ReadByte());
            otp.WriteByte((byte)ReadByte());
            List<byte>[] Snd = new List<byte>[256];
            for(int i = 0; i < 256; i++)
            {
                Snd[i] = new List<byte>();
            }
            if (sound)
            {
                for(int trk = 0; trk < trkcnt; trk++)
                {
                    ReadByte(); ReadByte(); ReadByte(); ReadByte();
                    long leng = 0;
                    for (int j = 0; j < 4; j++)
                    {
                        int x = toint(ReadByte());
                        leng = leng * 256 + x;
                    }
                    Console.WriteLine("Prereading track {0}/{1}, Size {2}", trk + 1, trkcnt, leng);
                    int lstcmd = 256;
                    int getnum()
                    {
                        int ans = 0;
                        int ch = 256;
                        while (ch >= 128)
                        {
                            ch = toint(ReadByte());
                            leng--;
                            ans = ans * 128 + (ch & 0b01111111);
                        }
                        return ans;
                    }
                    int get()
                    {
                        if (lstcmd != 256)
                        {
                            int lstcmd2 = lstcmd;
                            lstcmd = 256;
                            return lstcmd2;
                        }
                        leng--;
                        return toint(ReadByte());
                    }
                    int TM = 0;
                    int prvcmd = 256;
                    while (leng > 0)
                    {
                        TM += getnum();
                        int cmd = ReadByte();
                        leng--;
                        if (cmd < 128)
                        {
                            lstcmd = cmd;
                            cmd = prvcmd;
                        }
                        prvcmd = cmd;
                        int cm = cmd & 0b11110000;
                        if (cm == 0b10010000)
                        {
                            int nt = get();
                            byte vel = (byte)ReadByte();
                            while (Snd[nt].Count <= TM)
                            {
                                Snd[nt].Add((byte)0);
                            }
                            if (Snd[nt][TM] < vel)
                            {
                                Snd[nt][TM] = vel;
                            }
                            leng--;
                        }
                        else if (cm == 0b10000000)
                        {
                            int nt = get();
                            ReadByte();
                            leng--;
                        }
                        else if (cm == 0b11000000 || cm == 0b11010000 || cmd == 0b11110011)
                        {
                            get();
                        }
                        else if (cm == 0b11100000 || cm == 0b10110000 || cmd == 0b11110010 || cm == 0b10100000)
                        {
                            get();
                            ReadByte();
                            leng--;
                        }
                        else if (cmd == 0b11110000)
                        {
                            int ffx = get();
                            do
                            {
                                ffx = ReadByte();
                                leng--;
                            } while (ffx != 0b11110111);
                        }
                        else if (cmd == 0b11110100 || cmd == 0b11110001 || cmd == 0b11110101 || cmd == 0b11111001 || cmd == 0b11111101 || cmd == 0b11110110 || cmd == 0b11110111 || cmd == 0b11111000 || cmd == 0b11111010 || cmd == 0b11111100 || cmd == 0b11111110)
                        {
                        }
                        else if (cmd == 0b11111111)
                        {
                            cmd = get();
                            if (cmd == 0)
                            {
                                ReadByte(); ReadByte(); ReadByte();
                                leng-=3;
                            }
                            else if (cmd >= 1 && cmd <= 10 && cmd != 5 || cmd == 0x7f)
                            {
                                long ff = getnum();
                                while (ff-- > 0)
                                {
                                    ReadByte();
                                    leng--;
                                }
                            }
                            else if (cmd == 0x20 || cmd == 0x21)
                            {
                                ReadByte(); ReadByte(); leng -= 2;
                            }
                            else if (cmd == 0x2f)
                            {
                                ReadByte();
                                leng--;
                                break;
                            }
                            else if (cmd == 0x51)
                            {
                                ReadByte(); ReadByte(); ReadByte(); ReadByte();
                                leng -= 4;
                            }
                            else if (cmd == 5)
                            {
                                int ff = (int)getnum();
                                while (ff-- > 0)
                                {
                                    ReadByte();
                                    leng--;
                                }
                            }
                            else if (cmd == 0x54 || cmd == 0x58)
                            {
                                ReadByte(); ReadByte(); ReadByte(); ReadByte(); ReadByte();
                                leng -= 5;
                            }
                            else if (cmd == 0x59)
                            {
                                ReadByte(); ReadByte(); ReadByte();
                                leng -= 3;
                            }
                            else if (cmd == 0x0a)
                            {
                                int ss = get();
                                while (ss-- > 0)
                                {
                                    ReadByte();
                                    leng--;
                                }
                            }
                        }
                    }
                    while (leng > 0)
                    {
                        ReadByte();
                        leng--;
                    }
                }
                inp.Seek(14, SeekOrigin.Begin);
            }
            long ntc = 0, nte = 0;
            for (int trk = 0; trk < trkcnt; trk++)
            {
                otp.WriteByte((byte)ReadByte());
                otp.WriteByte((byte)ReadByte());
                otp.WriteByte((byte)ReadByte());
                otp.WriteByte((byte)ReadByte());
                long poss = inp.Position;
                long leng = 0;
                for (int j = 0; j < 4; j++)
                {
                    int x = toint(ReadByte());
                    leng = leng * 256 + x;
                }
                Console.WriteLine("Parsing track {0}/{1}, Size {2}", trk + 1, trkcnt, leng);
                ArrayList[] offs = new ArrayList[4096];
                for (int i = 0; i < 4096; i++)
                {
                    offs[i] = new ArrayList();
                }
                int[] ovo = new int[4096];
                for(int i = 0; i < 4096; i++)
                {
                    ovo[i] = 0;
                }
                int lstcmd = 256;
                int getnum()
                {
                    int ans = 0;
                    int ch = 256;
                    while (ch >= 128)
                    {
                        ch = toint(ReadByte());
                        leng--;
                        ans = ans * 128 + (ch & 0b01111111);
                    }
                    return ans;
                }
                int get()
                {
                    if (lstcmd != 256)
                    {
                        int lstcmd2 = lstcmd;
                        lstcmd = 256;
                        return lstcmd2;
                    }
                    leng--;
                    return toint(ReadByte());
                }
                int TM = 0;
                int prvcmd = 256;
                while (leng > 0)
                {
                    TM += getnum();
                    int cmd = ReadByte();
                    leng--;
                    if (cmd < 128)
                    {
                        lstcmd = cmd;
                        cmd = prvcmd;
                    }
                    prvcmd = cmd;
                    int cm = cmd & 0b11110000;
                    if (cm == 0b10010000)
                    {
                        int ch = (cmd & 0b00001111);
                        int nt = get();
                        ReadByte();
                        ovo[ch * 256 + nt]++;
                        leng--;
                        ntc++;
                        nte++;
                    }
                    else if (cm == 0b10000000)
                    {
                        int ch = (cmd & 0b00001111);
                        int nt = get();
                        ReadByte();
                        leng--;
                        if (ovo[ch * 256 + nt] > 0)
                        {
                            offs[ch * 256 + nt].Add(TM);
                            ovo[ch * 256 + nt]--;
                        }
                    }
                    else if (cm == 0b11000000 || cm == 0b11010000 || cmd == 0b11110011)
                    {
                        get();
                    }
                    else if (cm == 0b11100000 || cm == 0b10110000 || cmd == 0b11110010 || cm == 0b10100000)
                    {
                        get();
                        ReadByte();
                        leng--;
                    }
                    else if (cmd == 0b11110000)
                    {
                        int ffx = get();
                        do
                        {
                            ffx = ReadByte();
                            leng--;
                        } while (ffx != 0b11110111);
                    }
                    else if (cmd == 0b11110100 || cmd == 0b11110001 || cmd == 0b11110101 || cmd == 0b11111001 || cmd == 0b11111101 || cmd == 0b11110110 || cmd == 0b11110111 || cmd == 0b11111000 || cmd == 0b11111010 || cmd == 0b11111100 || cmd == 0b11111110)
                    {
                    }
                    else if (cmd == 0b11111111)
                    {
                        cmd = get();
                        if (cmd == 0)
                        {
                            ReadByte(); ReadByte(); ReadByte();
                            leng-=3;
                        }
                        else if (cmd >= 1 && cmd <= 10 && cmd != 5 || cmd == 0x7f)
                        {
                            long ff = getnum();
                            while (ff-- > 0)
                            {
                                ReadByte();
                                leng--;
                            }
                        }
                        else if (cmd == 0x20 || cmd == 0x21)
                        {
                            ReadByte(); ReadByte(); leng -= 2;
                        }
                        else if (cmd == 0x2f)
                        {
                            ReadByte();
                            leng--;
                            break;
                        }
                        else if (cmd == 0x51)
                        {
                            ReadByte(); ReadByte(); ReadByte(); ReadByte();
                            leng -= 4;
                        }
                        else if (cmd == 5)
                        {
                            int ff = (int)getnum();
                            while (ff-- > 0)
                            {
                                ReadByte();
                                leng--;
                            }
                        }
                        if (cmd == 0x54 || cmd == 0x58 || cmd == 0x59)
                        {
                            int ff = ReadByte();
                            leng -= ff + 1;
                            while (ff > 0)
                            {
                                ReadByte();
                                ff--;
                            }
                        }
                        else if (cmd == 0x0a)
                        {
                            int ss = get();
                            while (ss-- > 0)
                            {
                                ReadByte();
                                leng--;
                            }
                        }
                    }
                }
                Console.WriteLine("All parsed notes: {0}", ntc);
                Console.WriteLine("Parse finished. Removing overlaps ...");
                int[] ign = new int[4096];
                List<byte> outstr = new List<byte>();
                inp.Seek(poss, SeekOrigin.Begin);
                leng = 0;
                for (int j = 0; j < 4; j++)
                {
                    int x = toint(ReadByte());
                    leng = leng * 256 + x;
                }
                lstcmd = 256;
                int getnumwithoutstr()
                {
                    int ans = 0;
                    int ch = 256;
                    while (ch >= 128)
                    {
                        ch = toint(ReadByte());
                        outstr.Add((byte)ch);
                        leng--;
                        ans = ans * 128 + (ch & 0b01111111);
                    }
                    return ans;
                }
                TM = 0;
                prvcmd = 256;
                long lsttm = 0;
                void outtimetooutstr(long tm)
                {
                    List<byte> str = new List<byte>();
                    str.Add((byte)(tm % 128));
                    tm /= 128;
                    while (tm > 0)
                    {
                        str.Add((byte)(tm % 128 + 128));
                        tm /= 128;
                    }
                    for (int i = str.Count - 1; i >= 0; i--)
                    {
                        outstr.Add(str[i]);
                    }
                }
                Node[] noww = new Node[256];
                int[] cntt = new int[4096];
                for (int i = 0; i < 256; i++)
                {
                    noww[i] = hds[i];
                }
                for(int i = 0; i < 4096; i++)
                {
                    ovo[i] = 0;
                }
                lstcmd = 256;
                while (leng > 0)
                {
                    TM += getnum();
                    int cmd = ReadByte();
                    leng--;
                    if (cmd < 128)
                    {
                        lstcmd = cmd;
                        cmd = prvcmd;
                    }
                    prvcmd = cmd;
                    int cm = cmd & 0b11110000;
                    if (cm == 0b10010000)
                    {
                        int ch = (cmd & 0b00001111);
                        int nt = get();
                        int now = cntt[ch * 256 + nt];
                        cntt[ch * 256 + nt]++;
                        ovo[ch * 256 + nt]++;
                        int id = ch * 256 + nt;
                        if (offs[id].Count <= now)
                        {
                            outtimetooutstr(TM - lsttm);
                            lsttm = TM;
                            outstr.Add((byte)cmd);
                            outstr.Add((byte)nt);
                            if (sound)
                            {
                                outstr.Add(Convert.ToByte(Snd[nt][TM]));
                                ReadByte();
                            }
                            else
                            {
                                outstr.Add((byte)ReadByte());
                            }
                            leng--;
                            continue;
                        }
                        int offtm = Convert.ToInt32(offs[id][now]);
                        bool hvon = false;
                        int ntf = nt / 64;
                        while (hno[ntf].Count <= TM)
                        {
                            hno[ntf].Add(0);
                        }
                        if (((Convert.ToUInt64(hno[nt / 64][TM]) >> (nt % 64)) & 1) == 1)
                        {
                            hvon = true;
                        }
                        while (noww[nt].r != -1 && noww[nt].r < TM)
                        {
                            noww[nt] = noww[nt].nxt;
                        }
                        if (noww[nt].l <= TM && noww[nt].r >= offtm && hvon)
                        {
                            ign[id]++;
                        }
                        hno[nt / 64][TM] = Convert.ToUInt64(hno[nt / 64][TM]) | (1uL << (nt % 64));
                        if (noww[nt].r == -1 || noww[nt].l > offtm)
                        {
                            Node nn = new Node(TM, offtm);
                            Node mm = noww[nt];
                            nn.lst = mm.lst;
                            if (nn.lst != null)
                            {
                                nn.lst.nxt = nn;
                            }
                            else
                            {
                                hds[nt] = nn;
                            }
                            noww[nt] = nn;
                            nn.nxt = mm;
                            mm.lst = nn;
                        }
                        else if (noww[nt].r < offtm)
                        {
                            noww[nt].r = offtm;
                            Node nnow = noww[nt].nxt;
                            while (nnow.l != -1 && nnow.r <= noww[nt].r)
                            {
                                noww[nt].nxt = noww[nt].nxt.nxt;
                                nnow = noww[nt].nxt;
                                noww[nt].nxt.lst = noww[nt];
                            }
                            if (nnow.l != -1 && nnow.l <= noww[nt].r && nnow.r >= noww[nt].r)
                            {
                                noww[nt].r = nnow.r;
                                noww[nt].nxt = noww[nt].nxt.nxt;
                                noww[nt].nxt.lst = noww[nt];
                            }
                        }
                        if (noww[nt].l > TM)
                        {
                            noww[nt].l = TM;
                        }
                        if (ign[id] == 0)
                        {
                            outtimetooutstr(TM - lsttm);
                            lsttm = TM;
                            outstr.Add((byte)cmd);
                            outstr.Add((byte)nt);
                            outstr.Add((byte)ReadByte());
                        }
                        else
                        {
                            nte--;
                            ReadByte();
                        }
                        leng--;
                    }
                    else if (cm == 0b10000000)
                    {
                        int ch = (cmd & 0b00001111);
                        int nt = get();
                        int id = ch * 256 + nt;
                        if (ovo[id] > 0)
                        {
                            if (ign[id] == 0)
                            {
                                outtimetooutstr(TM - lsttm);
                                lsttm = TM;
                                outstr.Add((byte)cmd);
                                outstr.Add((byte)nt);
                                outstr.Add((byte)ReadByte());
                            }
                            else
                            {
                                ReadByte();
                                ign[id]--;
                            }
                            leng--;
                            ovo[id]--;
                        }
                        else
                        {
                            ReadByte();
                            leng--;
                        }
                    }
                    else if (cm == 0b11000000 || cm == 0b11010000 || cmd == 0b11110011)
                    {
                        outtimetooutstr(TM - lsttm);
                        lsttm = TM;
                        outstr.Add((byte)cmd);
                        outstr.Add((byte)get());
                    }
                    else if (cm == 0b11100000 || cm == 0b10110000 || cmd == 0b11110010 || cm == 0b10100000)
                    {
                        outtimetooutstr(TM - lsttm);
                        lsttm = TM;
                        outstr.Add((byte)cmd);
                        outstr.Add((byte)get());
                        outstr.Add((byte)ReadByte());
                        leng--;
                    }
                    else if (cmd == 0b11110000)
                    {
                        outtimetooutstr(TM - lsttm);
                        lsttm = TM;
                        outstr.Add((byte)cmd);
                        int ffx = get();
                        outstr.Add((byte)ffx);
                        do
                        {
                            ffx = ReadByte();
                            outstr.Add((byte)ffx);
                            leng--;
                        } while (ffx != 0b11110111);
                    }
                    else if (cmd == 0b11110100 || cmd == 0b11110001 || cmd == 0b11110101 || cmd == 0b11111001 || cmd == 0b11111101 || cmd == 0b11110110 || cmd == 0b11110111 || cmd == 0b11111000 || cmd == 0b11111010 || cmd == 0b11111100 || cmd == 0b11111110)
                    {
                        outtimetooutstr(TM - lsttm);
                        lsttm = TM;
                        outstr.Add((byte)cmd);
                    }
                    else if (cmd == 0b11111111)
                    {
                        outtimetooutstr(TM - lsttm);
                        lsttm = TM;
                        outstr.Add((byte)cmd);
                        cmd = get();
                        outstr.Add((byte)cmd);
                        if (cmd == 0)
                        {
                            outstr.Add((byte)ReadByte()); outstr.Add((byte)ReadByte()); outstr.Add((byte)ReadByte());
                            leng-=3;
                        }
                        else if (cmd >= 1 && cmd <= 10 && cmd != 5 || cmd == 0x7f)
                        {
                            long ff = getnumwithoutstr();
                            while (ff-- > 0)
                            {
                                outstr.Add((byte)ReadByte());
                                leng--;
                            }
                        }
                        else if (cmd == 0x20 || cmd == 0x21)
                        {
                            outstr.Add((byte)ReadByte()); outstr.Add((byte)ReadByte()); leng -= 2;
                        }
                        else if (cmd == 0x2f)
                        {
                            outstr.Add((byte)ReadByte());
                            leng--;
                            break;
                        }
                        else if (cmd == 0x51)
                        {
                            outstr.Add((byte)ReadByte()); outstr.Add((byte)ReadByte()); outstr.Add((byte)ReadByte()); outstr.Add((byte)ReadByte());
                            leng -= 4;
                        }
                        else if (cmd == 5)
                        {
                            int ff = (int)getnumwithoutstr();
                            while (ff-- > 0)
                            {
                                outstr.Add((byte)ReadByte());
                                leng--;
                            }
                        }
                        else if (cmd == 0x54 || cmd == 0x58 || cmd == 0x59)
                        {
                            int ff = ReadByte();
                            leng -= ff + 1;
                            outstr.Add((byte)ff);
                            while (ff > 0)
                            {
                                outstr.Add((byte)ReadByte());
                                ff--;
                            }
                        }
                        else if (cmd == 0x0a)
                        {
                            int ss = get();
                            outstr.Add((byte)ss);
                            while (ss-- > 0)
                            {
                                outstr.Add((byte)ReadByte());
                                leng--;
                            }
                        }
                    }
                }
                Console.WriteLine("Removed overlaps. All remaining notes: {0}/{1}, overlap rate: {2}%", nte, ntc, 100.0 * nte / ntc);
                while (leng > 0)
                {
                    ReadByte();
                    leng--;
                }
                long len = outstr.Count;
                byte[] Len = { (byte)(len / 256 / 256 / 256), (byte)(len / 256 / 256 % 256), (byte)(len / 256 % 256), (byte)(len % 256) };
                otp.Write(Len, 0, 4);
                var arr = outstr.ToArray();
                otp.Write(arr, 0, arr.Length);
            }
            otp.Flush();
            Console.WriteLine("Finished, press any key to exit ...");
            Console.ReadKey();
        }
        [STAThread]
        static void Main(string[] args)
        {
            Console.Title = ("Conjac Jelly Charlieyan's Overlap Remover Version 2.2.0.20191212");
            if (args.Length > 0)
            {
                filein = args[0];
            }
            else
            {
                Console.WriteLine("Select MIDI");
                var open = new OpenFileDialog();
                open.Filter = "Midi files (*.mid, *.midi)|*.mid; *.midi";
                if ((bool)open.ShowDialog())
                {
                    filein = open.FileName;
                }
                else return;
            }
            if (args.Length > 1)
            {
                fileout = args[1];
            }
            else
            {
                var ext = Path.GetExtension(filein);
                fileout = filein.Substring(0, filein.Length - ext.Length) + ".cjcor";
            }
            MessageBoxResult isaud = MessageBox.Show("Do you want to use audio render? Audio render can keep the sound but uses more time and RAM.", "Use audio render?", MessageBoxButton.YesNo);
            String res = isaud.ToString();
            Console.WriteLine("MessageBox returned {0}", res);
            bool sndd = false;
            if (res == "Yes")
            {
                fileout += ".aud.mid";
                sndd = true;
            }
            else
            {
                fileout += ".mid";
            }
            fastrender(sndd);
        }
    }
}
