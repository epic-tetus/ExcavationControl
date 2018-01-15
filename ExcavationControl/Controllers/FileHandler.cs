using ExcavationControl.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcavationControl.Controllers
{
    public class FileHandler
    {
        string defaultPath = "C:/ExcavationControlSystem";
        string basePath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName; // 프로젝트 폴더
        string fileName = "test.txt";

        public bool CreateFile()
        {
            try
            {
                File.Open(basePath += fileName, FileMode.Create);

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool OpenFile()
        {
            try
            {
                if(Directory.Exists(basePath))
                    File.Open(basePath += fileName,FileMode.OpenOrCreate);
                else
                {
                    //Directory.CreateDirectory(basePath);
                    Debug.WriteLine("디렉토리 없음");
                }

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool WriteText(string[] value)
        {
            try
            {
                System.IO.File.WriteAllLines(basePath, value, Encoding.Default);

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public bool AppendText(string[] value)
        {
            try
            {
                System.IO.File.AppendAllLines(basePath, value, Encoding.Default);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public FileHandler(string name)
        {
            fileName = name;
        }

        public FileHandler(string path, string name)
        {
            defaultPath = Path.Combine(basePath, path);
            fileName = name;
        }
    }
}
