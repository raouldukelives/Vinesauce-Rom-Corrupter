using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Vinesauce_ROM_Corruptor {
    class RomId {
        static SHA256 sha = SHA256Managed.Create();
        public string fullPath, readableHash, base64Hash;
        byte[] hash;
        public RomId(string fullPath) {
            this.fullPath = fullPath;
            byte[] ROM = ReadROM();
            this.hash = sha.ComputeHash(ROM);
            readableHash = BitConverter.ToString(hash);
            base64Hash = Convert.ToBase64String(hash);
        }
        string Filename() {
            return Path.GetFileName(fullPath);
        }

        public ListViewItem GetListViewItem() {
            ListViewItem item = new ListViewItem(new string[] { Filename(), readableHash });
            item.Tag = this;

            return item;
        }

        public byte[] ReadROM() {
            try {
                return File.ReadAllBytes(fullPath);
            } catch {
                MainForm.ShowErrorBox("Error reading ROM.");
                return null;
            }
        }

        public bool MatchesHash(byte[] otherHash) {
            if(otherHash == null) {
                return false;
            }
            return otherHash.SequenceEqual(hash);
        }
    }
}
