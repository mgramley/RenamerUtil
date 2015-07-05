using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerUtil
{
    public class Renamer
    {
        private List<string> _badChars = new List<string>() { "@", "#", "$", "%", "_", "-", "*", "%", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "(", ")", "'", "."};
        private List<string> _badStrings = new List<string>() { "season", "Season", "episdoe", "Episode", "[1080p]", "[1080P]", "1080P", "1080p", "1080", "[720p]", "[720P]", "720P", "720p", "720", "[480p]", "[480P]", "480P", "480p", "480" };

        public void PrintFileNames()
        {
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
            IEnumerable<FileInfo> infos = d.GetFiles().OrderBy(GetFileNameWithoutExt).ToList();
            foreach(FileInfo f in infos)
            {
                Console.WriteLine("Name: " + f.Name);
            }
        }

        public void RenameFilesInDir(List<string> args, bool keep)
        {
            string prefix;
            int season = 1;
            int episode = 1;
            if (args.Count > 0)
            {
                prefix = args[0];
            }
            else
            {
                prefix = string.Empty;
            }

            if (args.Count > 1)
            {
                season = Convert.ToInt32(args[1]);
            }

            if (args.Count > 2)
            {
                episode = Convert.ToInt32(args[2]);
            }
               
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
            IEnumerable<FileInfo> infos = d.GetFiles().OrderBy(GetFileNameWithoutExt).ToList();
            foreach (FileInfo f in infos)
            {
                string exten = f.Extension;
                string nameWithoutExtenstion = GetFileNameWithoutExt(f);
                string newName = FormatName(nameWithoutExtenstion, prefix, episode, season, exten, keep);

                if (f.DirectoryName != null && !string.IsNullOrWhiteSpace(f.DirectoryName))
                {
                    
                    string newPath = Path.Combine(f.DirectoryName, newName);
                    File.Move(f.FullName, newPath);
                    Console.WriteLine("Renaming the file: " + f.Name + "\r\n to: " + newName);
                }
                else
                {
                    Console.WriteLine("Could not rename file: " + f.Name);
                }

                episode++;
            }
        }

        public void AddExtension(string extension)
        {
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
            IEnumerable<FileInfo> infos = d.GetFiles().OrderBy(GetFileNameWithoutExt).ToList();

            foreach (FileInfo f in infos)
            {
                string nameWithoutExtenstion = GetFileNameWithoutExt(f);

                if (f.DirectoryName != null && !string.IsNullOrWhiteSpace(f.DirectoryName))
                {
                    string newPath = Path.Combine(f.DirectoryName, nameWithoutExtenstion + extension);
                    File.Move(f.FullName, newPath);
                    Console.WriteLine("Adding extension: " + extension + " Result: " + nameWithoutExtenstion + extension);
                }
                else
                {
                    Console.WriteLine("Could not rename file: " + f.Name);
                }
            }
        }

        public void RemoveString(List<string> phrases)
        {
            foreach (var phrase in phrases)
            {
                this.Remove(phrase);
            }
        }

        private void Remove(string phraseToRemove)
        {
            DirectoryInfo d = new DirectoryInfo(Directory.GetCurrentDirectory());
            IEnumerable<FileInfo> infos = d.GetFiles().OrderBy(GetFileNameWithoutExt).ToList();
            foreach (FileInfo f in infos)
            {
                string exten = f.Extension;
                string nameWithoutExtenstion = GetFileNameWithoutExt(f);
                string newName = nameWithoutExtenstion.Replace(phraseToRemove, string.Empty);

                if (f.DirectoryName != null && !string.IsNullOrWhiteSpace(f.DirectoryName))
                {

                    string newPath = Path.Combine(f.DirectoryName, newName + exten);
                    File.Move(f.FullName, newPath);
                    Console.WriteLine("Removing the phrase: " + phraseToRemove + " from: " + f.Name + "\r\n NewName: " + newName);
                }
                else
                {
                    Console.WriteLine("Could not rename file: " + f.Name);
                }
            }
        }

        private string FormatName(string orginalName, string prefix, int episode, int season, string extension, bool keep)
        {
            orginalName = _badStrings.Aggregate(orginalName, (current, badString) => current.Replace(badString, string.Empty));
            orginalName = _badChars.Aggregate(orginalName, (current, badChar) => current.Replace(badChar, string.Empty));
            orginalName = orginalName.Replace(prefix, string.Empty);

            orginalName = orginalName.Trim();

            if (!string.IsNullOrWhiteSpace(orginalName) && keep)
            {
                return prefix + " - s" + season.ToString("00") + "e" + episode.ToString("00") + " - " + orginalName + extension;
            }
            else
            {
                return prefix + " - s" + season.ToString("00") + "e" + episode.ToString("00") + extension;
            }
        }

        private static string GetFileNameWithoutExt(FileInfo f)
        {
            if(!string.IsNullOrWhiteSpace(f.Extension))
            {
                string s = f.Name.Replace(f.Extension, string.Empty);
                return s;
            }

            return f.Name;
        }
    }
}
