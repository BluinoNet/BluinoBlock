using BluinoNet.Modules.FatFsNano;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using static BluinoNet.Modules.FatFsNano.Ff;

namespace BluinoNet.Modules
{
    public class MicroSd
    {
        // c# port of FatFs: http://elm-chan.org/fsw/ff/00index_e.html

        public static void SetupMicroSd(int CsPin,int SpiBus=1)
        {
            MicroSdConfiguration.Setup(CsPin, SpiBus);
        }

        static FATFS fs = new FATFS();        /* FatFs work area needed for each volume */
        static FIL Fil = new FIL();           /* File object needed for each open file */

        static uint bw = 0;
        static FRESULT res;

        public static bool MountDrive()
        {
            res = Ff.Current.f_mount(ref fs, "", 1);     /* Give a work area to the default drive */

            if (res != FRESULT.FR_OK)
            {
                Debug.WriteLine(res.GetError());
                return false;
            }
            else
            {
                Debug.WriteLine("Drive successfully mounted");
              
            }
            return true;
        }

        /// <summary>
        /// create text file 
        /// </summary>
        /// <param name="FileName">/sub1/File1.txt</param>
        /// <param name="Content">content of file</param>
        public static bool CreateFile(string FileName, string Content)
        {
            var error = string.Empty;
            var result = true;
            if ((res = Ff.Current.f_open(ref Fil, FileName, FA_WRITE | FA_CREATE_ALWAYS)) == Ff.FRESULT.FR_OK)
            {   /* Create a file */
                Random rnd = new Random();
                var payload = Content.ToByteArray();
                res = Ff.Current.f_write(ref Fil, payload, (uint)payload.Length, ref bw);    /* Write data to the file */
                error = res.GetError();
                result = res == FRESULT.FR_OK;
                res = Ff.Current.f_close(ref Fil);   /* Close the file */
                error += res.GetError();
                result = res == FRESULT.FR_OK;
            }
            else
            {
                error = res.GetError();
                result = res == FRESULT.FR_OK;
            }
            Debug.WriteLine("File successfully created");
            return result;

        }

        /// <summary>
        /// read text file content
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string ReadFile(string FileName)
        {
            var error = string.Empty;
            var result = true;
            string content=string.Empty;
            if (Ff.Current.f_open(ref Fil, FileName, FA_READ) == Ff.FRESULT.FR_OK)
            {   /* Create a file */

                var newPayload = new byte[5000];
                res = Ff.Current.f_read(ref Fil, ref newPayload, 5000, ref bw);    /* Read data from file */
                error += res.GetError();
                result = res == FRESULT.FR_OK;

                content = Encoding.UTF8.GetString(newPayload, 0, (int)bw);
                Debug.WriteLine($"{content}");

                res = Ff.Current.f_close(ref Fil);                              /* Close the file */
                error += res.GetError();
                result = res == FRESULT.FR_OK;
            }

            Debug.WriteLine("File successfully read");
            return result ? content : "";
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="FileName">"/sub1/File2.txt"</param>
        public static bool DeleteFile(string FileName)
        {
            res = Ff.Current.f_unlink(FileName);     /* Give a work area to the default drive */
            var error = res.GetError();

            Debug.WriteLine("File successfully deleted");
            return res == FRESULT.FR_OK;
        }

        public static ScanFileResult ListDirectory()
        {

            res = Ff.Current.f_mount(ref fs, "", 1);
            var error = res.GetError();
            var result = new ScanFileResult();
            res = Scan_Files("/",out result);
            error += res.GetError();

            Debug.WriteLine("Directories successfully listed");
            return res == FRESULT.FR_OK? result : null;
        }

        private static FRESULT Scan_Files(string path, out ScanFileResult result)
        {
           
            ArrayList Directories=new ArrayList();
            ArrayList Files = new ArrayList();
            FRESULT res;
            FILINFO fno = new FILINFO();
            DIR dir = new DIR();
            byte[] buff = new byte[256];
            buff = path.ToNullTerminatedByteArray();

            res = Ff.Current.f_opendir(ref dir, buff);                      /* Open the directory */
            if (res == FRESULT.FR_OK)
            {
                for (; ; )
                {
                    res = Ff.Current.f_readdir(ref dir, ref fno);           /* Read a directory item */
                    if (res != FRESULT.FR_OK || fno.fname[0] == 0) break;   /* Break on error or end of dir */
                    if ((fno.fattrib & AM_DIR) > 0 && !((fno.fattrib & AM_SYS) > 0 || (fno.fattrib & AM_HID) > 0))
                    {
                        /* It is a directory */
                        var newpath = path + "/" + fno.fname.ToStringNullTerminationRemoved();
                        Debug.WriteLine($"Directory: {path}/{fno.fname.ToStringNullTerminationRemoved()}");
                        Directories.Add($"{path}/{fno.fname.ToStringNullTerminationRemoved()}");
                        var res2 = new ScanFileResult();
                        res = Scan_Files(newpath, out res2);                    /* Enter the directory */
                        if (res != FRESULT.FR_OK) break;
                        else
                        {
                            foreach(var file in res2.Files)
                            {
                                Files.Add(file);
                            }
                            foreach(var dir1 in res2.Directories)
                            {
                                Directories.Add(dir1);
                            }
                        }
                    }
                    else
                    {
                        /* It is a file. */
                        Debug.WriteLine($"File: {path}/{fno.fname.ToStringNullTerminationRemoved()}");
                        Files.Add($"{path}/{fno.fname.ToStringNullTerminationRemoved()}");

                    }
                }
                Ff.Current.f_closedir(ref dir);
            }
            result = new ScanFileResult();
            result.Files = (string[])Files.ToArray(typeof(string));
            result.Directories = (string[])Directories.ToArray(typeof(string));
            return res;
        }

        /// <summary>
        /// Create Directory
        /// </summary>
        /// <param name="FolderPath">sub1/sub2/sub3</param>
        public static bool CreateDirectory(string FolderPath)
        {
            var error = string.Empty;
            res = Ff.Current.f_mkdir("sub1");
            if (res != FRESULT.FR_EXIST)
            {
                error = res.GetError();
            }

            Debug.WriteLine("Directories successfully created");
            return res == FRESULT.FR_OK;
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        /// <param name="FileName">"/sub1/File2.txt"</param>
        /// <returns></returns>
        public static bool IsFileExist(string FileName)
        {
            var exists = false;
            FILINFO fno = new FILINFO();

            res = Ff.Current.f_stat(FileName, ref fno);
            switch (res)
            {

                case Ff.FRESULT.FR_OK:
                    Debug.WriteLine($"Size: {fno.fsize}");
                    Debug.WriteLine(String.Format("Timestamp: {0}/{1}/{2}, {3}:{4}",
                           (fno.fdate >> 9) + 1980, fno.fdate >> 5 & 15, fno.fdate & 31,
                           fno.ftime >> 11, fno.ftime >> 5 & 63));
                    Debug.WriteLine(String.Format("Attributes: {0}{1}{2}{3}{4}",
                           (fno.fattrib & AM_DIR) > 0 ? 'D' : '-',
                           (fno.fattrib & AM_RDO) > 0 ? 'R' : '-',
                           (fno.fattrib & AM_HID) > 0 ? 'H' : '-',
                           (fno.fattrib & AM_SYS) > 0 ? 'S' : '-',
                           (fno.fattrib & AM_ARC) > 0 ? 'A' : '-'));
                    exists = true;
                    break;

                case Ff.FRESULT.FR_NO_FILE:
                    Debug.WriteLine("File does not exist");
                    exists = false;
                    break;

                default:
                    exists = false;
                    Debug.WriteLine($"An error occured. {res.ToString()}");
                    break;
            }
            return exists;
        }

        public static uint GetFreeSpaceInKb()
        {

            uint fre_clust = 0;
            uint fre_sect, tot_sect;

            /* Get volume information and free clusters of drive 1 */
            res = Ff.Current.f_getfree("0:", ref fre_clust, ref fs);
            if (res != FRESULT.FR_OK)
            {
                Debug.WriteLine($"An error occured. {res.ToString()}");
                return 0;
            };

            /* Get total sectors and free sectors */
            tot_sect = (fs.n_fatent - 2) * fs.csize;
            fre_sect = fre_clust * fs.csize;

            /* Print the free space (assuming 512 bytes/sector) */
            var freeSpace =  fre_sect / 2;
            Debug.WriteLine(String.Format("{0} KB total drive space\n{1} KB available", tot_sect / 2, freeSpace));
            return freeSpace;
        }

        /// <summary>
        /// Rename file
        /// </summary>
        /// <param name="OldFileName">"/sub1/File1.txt"</param>
        /// <param name="NewFileName">"/sub1/File2.txt"</param>
        public static bool RenameFile(string OldFileName,string NewFileName)
        {
            /* Rename an object in the default drive */
            res = Ff.Current.f_rename(OldFileName, NewFileName);
            var error = res.GetError();

            Debug.WriteLine("File successfully renamed");
            return res == FRESULT.FR_OK;
        }
    }

    public class ScanFileResult
    {
        public string[] Directories { get; set; }
        public string[] Files { get; set; }
    }
}
