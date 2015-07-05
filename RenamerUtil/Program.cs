using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenamerUtil
{
    class Program
    {
        static void Main(string[] args)
        {
            Renamer r = new Renamer();

            if (args.Length > 0)
            {
                if (args[0] == "-t")
                {
                    r.PrintFileNames();
                }
                else if (args[0] == "-r")
                {
                    var a = args.ToList();
                    a.Remove(args[0]);
                    r.RenameFilesInDir(a, false);
                }
                else if (args[0] == "-rr")
                {
                    var a = args.ToList();
                    a.Remove(args[0]);
                    r.RenameFilesInDir(a, true);
                }
                else if(args[0] == "-remove")
                {
                    var a = args.ToList();
                    a.Remove(args[0]);
                    r.RemoveString(a);
                }
                else if(args[0] == "-addex")
                {
                    if (args.Count() > 1)
                    {
                        r.AddExtension(args[1]);
                    }
                }
                
            }


            //Console.ReadLine();
        }
    }
}
