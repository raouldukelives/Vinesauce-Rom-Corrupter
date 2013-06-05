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
            if(ROM != null) {
                this.hash = sha.ComputeHash(ROM);
                readableHash = BitConverter.ToString(hash);
                base64Hash = Convert.ToBase64String(hash);
            }
        }
        string FileName() {
            return Path.GetFileName(fullPath);
        }

        public ListViewItem GetListViewItem() {
            ListViewItem item = new ListViewItem(new string[] { FileName(), readableHash });
            item.Tag = this;

            return item;
        }

        public byte[] ReadROM() {
            try {
                return File.ReadAllBytes(fullPath);
            } catch {
                //this message might get annoying if there are a lot of files that fail to read
                MainForm.ShowErrorBox("Error reading file " + fullPath);
                return null;
            }
        }

        public bool MatchesHash(byte[] otherHash) {
            if(otherHash == null) {
                return false;
            }
            return otherHash.SequenceEqual(hash);
        }

        public bool MatchesFilename(string otherFullpath) {
            if(otherFullpath == null) {
                return false;
            }
            return Path.GetFileName(otherFullpath) == FileName();
        }
    }
}
