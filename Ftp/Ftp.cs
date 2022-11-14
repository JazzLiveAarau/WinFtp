using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;
using System.Net.NetworkInformation;

/// <summary>Upload and download of files with FTP</summary>
namespace Ftp
{
    /// <summary>Upload of files</summary>
    public class UpLoad
    {
        #region Member variables

        /// <summary>FTP host</summary>
        public string m_ftp_host = "";

        /// <summary>FTP user</summary>
        public string m_ftp_user = "";

        /// <summary>FTP password</summary>
        public string m_ftp_password = "";

        /// <summary>FTP start string</summary>
        public string m_ftp = "ftp://";

        #endregion // Member variables

        #region Constructor

        /// <summary>Constructor with host name, user and password as input</summary>
        public UpLoad(string i_ftp, string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp = i_ftp;
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;
        }

        /// <summary>Constructor with host name, user and password as input</summary>
        public UpLoad(string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;
        }

        /// <summary>Constructor</summary>
        public UpLoad()
        {
        }

        #endregion // Constructor

        #region Init (Set)

        /// <summary>Set host name, user and password</summary>
        public void Set(string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;

        } // Set

        /// <summary>Set ftp, host name, user and password</summary>
        public void Set(string i_ftp, string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp = i_ftp;
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;

        } // Set

        #endregion // Init (Set)

        #region Directory functions

        /// <summary>
        /// Create directory on the server
        /// </summary>
        /// <param name="i_directory_server">Name of the directory e.g. ftp://www.jazzliveaarau.ch/XML/Backup_2016-03-29_11_53_10</param>
        /// <param name="o_error">Error message for function failure</param>
        public bool CreateDirectory(string i_directory_server, out string o_error)
        {
            o_error = "";

            string directory_uri = m_ftp + m_ftp_host + "/" + i_directory_server;

            string ftp_response_str = "";

            WebRequest ftp_web_request = null; 
            try
            {
                ftp_web_request = WebRequest.Create(directory_uri);

                ftp_web_request.Method = WebRequestMethods.Ftp.MakeDirectory;

                ftp_web_request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();                    
                }

                o_error = "UpLoad.CreateDirectory No error ftp_response_str= " + ftp_response_str;
            }
            catch (Exception e)
            {
                o_error = "UpLoad.CreateDirectory FtpWebRequest failed: " + e.Message;
                return false;
            }

            return true;
        } // CreateDirectory

        /// <summary>
        /// Returns true if the server library already exists 
        /// <para></para>
        /// <para>Code from: https://stackoverflow.com/questions/2769137/how-to-check-if-an-ftp-directory-exists</para>
        /// </summary>
        /// <param name="i_directory_server">Name of the directory e.g. ftp://www.jazzliveaarau.ch/XML/Backup_2016-03-29_11_53_10</param>
        /// <param name="o_ftp_response">Response</param>
        public bool DoesDirectoryExist(string i_directory_server, out string o_ftp_response)
        {
            o_ftp_response = "";

            string directory_uri = m_ftp + m_ftp_host + "/" + i_directory_server;

            string ftp_response_str = "";

            WebRequest ftp_web_request = null;
            try
            {
                ftp_web_request = WebRequest.Create(directory_uri);

                ftp_web_request.Method = WebRequestMethods.Ftp.ListDirectory;

                ftp_web_request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                o_ftp_response = "Directory exists: Response= " + ftp_response_str;
            }
            catch (Exception e)
            {
                o_ftp_response = "Directory does not exist. Response= " + e.Message;
                return false;
            }

            return true;

        } // DoesDirectoryExist

        #endregion // Directory functions

        #region Upload

        /// <summary>
        /// Upload file binary mode
        /// </summary>
        /// <param name="i_file_server">Name of file in server</param>
        /// <param name="i_local_filename">Name of file in local computer</param>
        /// <param name="o_error">Error message for function failure</param>
        public bool UploadFile(string i_file_server, string i_local_filename, out string o_error)
        {
            // Code is exctracted from: 
            // http://www.codeproject.com/Articles/17202/Simple-FTP-demo-application-using-C-Net-2-0

            o_error = "";

            if (!File.Exists(i_local_filename))
            {
                o_error = "UpLoad.UploadFile Web There is no file " + i_local_filename;
                return false;
            }

            FtpWebRequest ftp_web_request = null;

            FileInfo file_info = new FileInfo(i_local_filename);
            string str_uri = m_ftp + m_ftp_host + "/" + i_file_server;

            Uri object_uri = new Uri(str_uri);

            try
            {
                ftp_web_request = (FtpWebRequest)FtpWebRequest.Create(object_uri);
            }
            catch (Exception e)
            {
                o_error = "UpLoad.UploadFile FtpWebRequest failed: " + e.Message;
                return false;
            }
            if (null == ftp_web_request)
            {
                o_error = "UpLoad.UploadFile Web ftp_web_request failed. Pointer is null.";
                return false;
            }

            // Provide the WebPermission Credentials
            ftp_web_request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

            // By default KeepAlive is true, where the control connection is not closed
            // after a command is executed.
            ftp_web_request.KeepAlive = false;

            // Specify the command to be executed.
            ftp_web_request.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            ftp_web_request.UseBinary = true;

            // Notify the server about the size of the uploaded file
            ftp_web_request.ContentLength = file_info.Length;

            // The buffer size is set to 2kb
            int buffer_size = 2048;
            byte[] read_buffer = new byte[buffer_size];
            int content_length;

            // Opens a file stream (System.IO.FileStream) to read the file to be uploaded
            FileStream file_stream = null;

            // Stream to which the file to be upload is written
            Stream ftp_stream = null;

            try
            {
                file_stream = file_info.OpenRead();

                ftp_stream = ftp_web_request.GetRequestStream();

                // Read from the file stream 2kb at a time
                content_length = file_stream.Read(read_buffer, 0, buffer_size);

                // Till Stream content ends
                while (content_length != 0)
                {
                    // Write Content from the file stream to the FTP Upload Stream
                    ftp_stream.Write(read_buffer, 0, content_length);
                    content_length = file_stream.Read(read_buffer, 0, buffer_size);
                }

                // Close the file stream and the Request Stream
                ftp_stream.Close();
                file_stream.Close();
            }
            catch (Exception ex)
            {
                o_error = "UpLoad.UploadFile Upload failed. " + ex.Message;
                return false;  

            }

            finally
            {
                if (null != ftp_stream) ftp_stream.Close();
                if (null != file_stream) file_stream.Close();
            }

            return true;

        } // UploadFile

        #endregion // Upload

        #region Delete

        /// <summary>Delete of files on a server directory</summary>
        public bool DeleteFiles(string i_directory_server, string[] i_server_filenames, out string o_error)
        {
            o_error = "";

            if (null == i_server_filenames || i_server_filenames.Length == 0)
            {
                o_error = @"Upload.DeleteFiles i_server_filenames is null or has no elements";
                return false;
            }

            for (int index_name=0; index_name< i_server_filenames.Length; index_name++)
            {
                string server_file_name = i_server_filenames[index_name];

                if (!DeleteFile(i_directory_server, server_file_name, out o_error))
                {
                    o_error = @"Upload.DeleteFiles DeleteFile failed " + o_error;
                    return false;
                }

            } // index_name


            return true;

        } // DeleteFiles

        /// <summary>Delete of one file</summary>
        public bool DeleteFile(string i_directory_server, string i_server_filename, out string o_error)
        {
            o_error = "";

            string directory_uri = m_ftp + m_ftp_host + i_directory_server + "/" + i_server_filename;

            string ftp_response_str = "";

            WebRequest ftp_web_request = null;
            try
            {
                ftp_web_request = WebRequest.Create(directory_uri);

                ftp_web_request.Method = WebRequestMethods.Ftp.DeleteFile;

                ftp_web_request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                using (FtpWebResponse ftp_response = (FtpWebResponse)ftp_web_request.GetResponse())
                {
                    ftp_response_str = ftp_response.StatusCode.ToString();
                }

                o_error = "UpLoad.DeleteFile No error ftp_response_str= " + ftp_response_str;
            }
            catch (Exception e)
            {
                o_error = "UpLoad.DeleteFile FtpWebRequest failed: " + e.Message;
                return false;
            }

            return true;

        } // DeleteFile

        #endregion // Delete

    } // Upload

    /// <summary>Download of files</summary>
    public class DownLoad
    {
        #region Member variables

        /// <summary>FTP host</summary>
        public string m_ftp_host = "";

        /// <summary>FTP user</summary>
        public string m_ftp_user = "";

        /// <summary>FTP password</summary>
        public string m_ftp_password = "";

        /// <summary>FTP start string</summary>
        public string m_ftp = "ftp://";

        #endregion // Member variables

        #region Constructor and Set

        /// <summary>Constructor with host name, user and password as input</summary>
        public DownLoad(string i_ftp_host, string i_ftp_user,  string i_ftp_password)
        {
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;
        }

        /// <summary>Constructor with ftp start string, host name, user and password as input</summary>
        public DownLoad(string i_ftp, string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp = i_ftp;
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;
        }


        /// <summary>Constructor</summary>
        public DownLoad()
        {
        }

        /// <summary>Set host name, user and password</summary>
        public void Set(string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;
        } // Set

        /// <summary>Set ftp start string, host name, user and password</summary>
        public void Set(string i_ftp, string i_ftp_host, string i_ftp_user, string i_ftp_password)
        {
            m_ftp = i_ftp;
            m_ftp_host = i_ftp_host;
            m_ftp_user = i_ftp_user;
            m_ftp_password = i_ftp_password;

        } // Set

        #endregion // Constructor and Set

        #region Utility functions

        /// <summary>Convert directory content string to an array of file names. Directories are not returned</summary>
        private string[] _DirectoryContentToArray(string i_directory_list)
        {
            string[] ret_array = null;

            ArrayList ret_array_list = new ArrayList();

            bool b_file_name = false;
            string file_name = "";

            string directory_list = i_directory_list.Substring(6);

            for (int i_char = 0; i_char < directory_list.Length; i_char++)
            {
                string current_char = directory_list.Substring(i_char, 1);

                if ("\n" ==  current_char && false == b_file_name)
                {
                    b_file_name = true;
                    file_name = "";
                }
                else if ("\r" == current_char && true == b_file_name)
                {
                    b_file_name = false;
                    if (!IfDirectory(file_name))
                    {
                        ret_array_list.Add(file_name);
                    }
                   
                    file_name = "";
                }
                else if (true == b_file_name)
                {
                    file_name = file_name + current_char;
                }

            }
            /*
".\r\n..\r\nPlakatNewsLetterJubilaeum_B.jpg\r\nPlakatNewsLetterJubilaeum_D.jpg\r\nPlakatNewsletter20120211.jpg\r\nPlakatNewsletter20120225.jpg\r\nPlakatNewsletter20120310.jpg\r\nPlakatNewsletter20120324.jpg\r\nPlakatNewsletterJubilaeum_F.jpg\r\n"
  * 

 */
            ret_array = (string[])ret_array_list.ToArray(typeof(string));

            return ret_array;
        } // _DirectoryContentToArray

        /// <summary>Determines wether it is a directory or a file. Bad criterion is used. Point in file name!</summary>
        private bool IfDirectory(string i_file_name)
        {
            if (i_file_name.Contains(@"."))
                return false;
            else
                return true;
        } // IfDirectory

        #endregion // Utility functions

        #region Check Internet connection

        /// <summary>Get Internet connection test file name. Returns true if connection works</summary>
        public bool CheckInternetConnectionWithTestFileNames(string i_test_directory_server, out string o_status)
        {
            o_status = @"";

            FtpWebResponse response = null;
            StreamReader reader = null;
            string directory_list = "";

            try
            {
                string str_uri = m_ftp + m_ftp_host + i_test_directory_server;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(str_uri);

                request.Method = WebRequestMethods.Ftp.ListDirectory;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                reader = new StreamReader(responseStream);
                directory_list = reader.ReadToEnd();

                reader.Close();
                response.Close();

            }

            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

                o_status = ex.Message;

                return false;
            }

            directory_list = directory_list + "";

            string[] file_name_array = _DirectoryContentToArray(directory_list);

            if (file_name_array.Length >= 1)
            {
                o_status = @"Test file name= " + file_name_array[0];
            }

            return true;

        } // CheckInternetConnectionWithTestFileNames

        #endregion // Check Internet connection

        #region Download multiple files

        /// <summary>Download of files from one server directory to a local directory</summary>
        public bool GetFiles(string i_directory_server, string i_directory_local, out string o_error)
        {
            o_error = "";

            FtpWebResponse response = null;
            StreamReader reader = null;
            string directory_list = "";

            try
            {
                string str_uri = m_ftp + m_ftp_host + i_directory_server;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(str_uri);
              
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                reader = new StreamReader(responseStream);
                directory_list = reader.ReadToEnd();

                reader.Close();
                response.Close();

            }

            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

                o_error = ex.Message;

                return false;
            }

            directory_list = directory_list + "";

            string[] file_name_array = _DirectoryContentToArray(directory_list);

            for (int i_file = 0; i_file < file_name_array.Length; i_file++)
            {
                string server_file_name = i_directory_server + file_name_array[i_file];
                string local_file_name = i_directory_local + @"\" + file_name_array[i_file];

                if (!DownloadFile(server_file_name, local_file_name, out o_error))
                {
                    return false;
                }
            }

            return true;
        } // GetFiles

        /// <summary>Download of files from a server directory to a local directory</summary>
        public bool DownloadFilesServerLocal(string i_directory_server, string i_directory_local, string[] i_file_name_array, out string o_error)
        {
            o_error = "";

            for (int index_file = 0; index_file < i_file_name_array.Length; index_file++)
            {
                string file_name = i_file_name_array[index_file];

                if (!DownloadFileServerLocal(i_directory_server, i_directory_local, file_name, out o_error))
                {
                    return false;
                }
            }

            return true;

        } // DownloadFilesServerLocal

        #endregion // Download multiple files

        #region Get file names on a server directory

        /// <summary>Get file names from one directory. TODO Check if this function is used by any application</summary>
        public bool GetFileNames(string i_directory_server, string i_directory_local, out string[] o_file_name_array, out string o_error)
        {
            o_error = "";

            ArrayList file_names_array = new ArrayList();
            o_file_name_array = (string[])file_names_array.ToArray(typeof(string));

            FtpWebResponse response = null;
            StreamReader reader = null;
            string directory_list = "";

            try
            {
                string str_uri = m_ftp + m_ftp_host + i_directory_server;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(str_uri);

                request.Method = WebRequestMethods.Ftp.ListDirectory;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                reader = new StreamReader(responseStream);
                directory_list = reader.ReadToEnd();

                reader.Close();
                response.Close();

            }

            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

                o_error = ex.Message;

                return false;
            }

            directory_list = directory_list + "";

            o_file_name_array = _DirectoryContentToArray(directory_list);

            return true;

        } // GetFileNames

        /// <summary>Get file names from a server directory
        /// <para>From www directory use: FtpUser and FtpWwwPassword</para>
        /// <para></para>
        /// </summary>
        /// <param name="i_directory_server">The server directory name, e.g /www/Audio/REG00001/AudioOne/</param>
        /// <param name="o_file_name_array">Returned file names. Only names and no paths</param>
        /// <param name="o_error">Error message</param>
        public bool GetServerDirectoryFileNames(string i_directory_server, out string[] o_file_name_array, out string o_error)

        {
            o_error = "";

            ArrayList file_names_array = new ArrayList();
            o_file_name_array = (string[])file_names_array.ToArray(typeof(string));

            FtpWebResponse response = null;
            StreamReader reader = null;
            string directory_list = "";

            try
            {
                string str_uri = m_ftp + m_ftp_host + i_directory_server;
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(str_uri);

                request.Method = WebRequestMethods.Ftp.ListDirectory;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                reader = new StreamReader(responseStream);
                directory_list = reader.ReadToEnd();

                reader.Close();
                response.Close();

            }

            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

                o_error = ex.Message;

                return false;
            }

            directory_list = directory_list + "";

            o_file_name_array = _DirectoryContentToArray(directory_list);

            return true;
        } // GetServerDirectoryFileNames

        #endregion // Get file names on a server directory

        #region Download one file

        private string ReplaceSpaces(string i_string)
        {
            string ret_str = @"";

            /*
            if (i_string.Contains(" "))
            {
                ret_str = "\"" + i_string + "\""; 
            }
            else
            {
                ret_str = i_string;
            }
            */

            ret_str = i_string.Replace(" ", Uri.HexEscape(' '));


            return ret_str;

        } //

        
        public bool DownloadFileWebClient(string i_file_server, string i_local_filename, out string o_error)
        {
            o_error = "";

            string remoteUri = "http://www.contoso.com/library/homepage/images/";
            remoteUri = m_ftp + m_ftp_host + "/";

            string fileName = "ms-banner.gif", myStringWebResource = null;

            fileName = i_file_server;

            try
            {
                // Create a new WebClient instance.
                using (WebClient myWebClient = new WebClient())
                {
                    myWebClient.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);
                    myStringWebResource = remoteUri + fileName;
                    // Download the Web resource and save it into the current filesystem folder.
                    // myWebClient.DownloadFile(myStringWebResource, fileName);
                    myWebClient.DownloadFile(myStringWebResource, i_local_filename);
                }
            }
            catch (Exception e)
            {
                o_error = e.Message;
                return false;
            }

            return true;
        }
        

        /// <summary>Download of one file
        /// <para>ftp://m_ftp_host/ is added to input server file name, e.g ftp://www.jazzliveaarau.ch/www/Audio/REG00011/AudioOne/Tune_1.mp3</para>
        /// <para></para>
        /// <para></para>
        /// <para></para>
        /// </summary>
        /// <param name="i_file_server">The server file name e.g. /www/Audio/REG00011/AudioOne/Tune_1.mp3</param>
        /// <param name="i_directory_local">The local directory e.g.C:\Apps\JazzLiveAarau\Admin\Dokumente\Tune_1.mp3</param>
        /// <param name="o_error">Error message</param>
        public bool  DownloadFile(string i_file_server, string i_local_filename, out string o_error)
        {
            o_error = "";

            /*
            if (i_file_server.Contains(@" "))
            {
                if (!DownloadFileWebClient(i_file_server, i_local_filename, out o_error))
                {
                    return false;
                }

                return true;
            }
           */

            bool b_return = true;

            int bytes_processed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream remote_stream = null;
            Stream local_stream = null;
            WebResponse web_response = null;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
                // string file_server = ReplaceSpaces(i_file_server);

                // Create a ftp_web_request for the specified remote file name
                string str_uri = m_ftp + m_ftp_host + "/" + i_file_server;
                Uri uri_file_server = new Uri(str_uri);
                bool b_escaped = uri_file_server.UserEscaped;
                string scheme = uri_file_server.Scheme;
                WebRequest web_request = WebRequest.Create(uri_file_server);

                // WebRequest web_request = WebRequest.Create(str_uri);


                if (web_request != null)
                {
                    web_request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                    // Send the ftp_web_request to the server and retrieve the
                    // WebResponse object 
                    web_response = web_request.GetResponse();
                    if (web_response != null)
                    {
                        // Once the WebResponse object has been retrieved,
                        // get the stream object associated with the web_response's data
                        remote_stream = web_response.GetResponseStream();

                        // Create the local file
                        local_stream = File.Create(i_local_filename);

                        // Allocate a 1k buffer
                        byte[] buffer = new byte[1024];
                        int bytes_read;

                        // Simple do/while loop to read from stream until
                        // no bytes are returned
                        do
                        {
                            // Read data (up to 1k) from the stream
                            bytes_read = remote_stream.Read(buffer, 0, buffer.Length);

                            // Write the data to the local file
                            local_stream.Write(buffer, 0, bytes_read);

                            // Increment total bytes processed
                            bytes_processed += bytes_read;
                        } while (bytes_read > 0);
                    }
                }
            }
            catch (Exception e)
            {
                o_error = e.Message;
                string status = e.Source;
                //String status = ((FtpWebResponse)e.Response).StatusDescription;
                b_return = false;
            }
            finally
            {
                // Close the web_response and streams objects here 
                // to make sure they're closed even if an exception
                // is thrown at some point
                if (web_response != null) web_response.Close();
                if (remote_stream != null) remote_stream.Close();
                if (local_stream != null) local_stream.Close();
            }

            return b_return;

        } // DownloadFile

        /// <summary>Download of one file binary mode</summary>
        public bool DownloadBinary(string i_file_server, string i_local_filename, out string o_error)
        {
            // Code is exctracted from: 
            // http://www.codeproject.com/Articles/17202/Simple-FTP-demo-application-using-C-Net-2-0

            o_error = "";
        
            FtpWebRequest ftp_web_request = null;
            try
            {
                string str_uri = m_ftp + m_ftp_host + "/" + i_file_server;
                ftp_web_request = (FtpWebRequest)FtpWebRequest.Create(str_uri);
            }
            catch (Exception e)
            {
                o_error = e.Message;
                return false;
            }

            if (null == ftp_web_request)
            {
                o_error = "DownLoad.DownloadBinary Web ftp_web_request failed";
                return false;
            }

            FtpWebResponse ftp_web_response = null;
            Stream ftp_response_stream = null;
            FileStream output_stream = null;

            bool b_return = true;

            try
            {
                ftp_web_request.Method = WebRequestMethods.Ftp.DownloadFile;
                ftp_web_request.UseBinary = true;

                ftp_web_request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                ftp_web_response = (FtpWebResponse)ftp_web_request.GetResponse();

                ftp_response_stream = ftp_web_response.GetResponseStream();

                long content_length = ftp_web_response.ContentLength;

                int buffer_size = 2048;

                int read_count;

                byte[] read_buffer = new byte[buffer_size];

                output_stream = new FileStream(i_local_filename, FileMode.Create);

                read_count = ftp_response_stream.Read(read_buffer, 0, buffer_size);
                while (read_count > 0)
                {
                    output_stream.Write(read_buffer, 0, read_count);
                    read_count = ftp_response_stream.Read(read_buffer, 0, buffer_size);
                }
            }

            catch (Exception e)
            {
                o_error = e.Message;
                b_return = false;
            }

            finally
            {
                if (null != ftp_response_stream) ftp_response_stream.Close();
                if (null != output_stream) output_stream.Close();
                if (null != ftp_response_stream) ftp_response_stream.Close();
            }

            return b_return;

        } // DownloadBinary

        /// <summary>Download of one file</summary>
        /// <param name="i_file_server">Name of the file on the server</param>
        /// <param name="i_file_local">Local file that will be created</param>
        /// <param name="i_file_encoding">Encoding that will be used. If null Encoding.Default will be used</param>
        /// <param name="o_error">Error message for failure</param>
        public bool GetFile(string i_file_server, string i_file_local, Encoding i_file_encoding, out string o_error)
        {
            o_error = "";

            FtpWebResponse response = null;
            StreamReader reader = null;
            string file_content = "";

            try
            {
                // Get the object used to communicate with the server.
                string str_uri = m_ftp + m_ftp_host + "/" + i_file_server;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(str_uri);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                request.Credentials = new NetworkCredential(m_ftp_user, m_ftp_password);

                response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                // Without System.Text.Encoding.Default there are problems with ä ö ü QQQqqqq Default OK (and none from the others) here but not below ... ???????????????????????????
                // Without System.Text.Encoding.UTF8 there are problems with ä ö ü. With Encoding.Default it worked in some computers
                // Alternatives Encoding.Default, Encoding.UTF8, Encoding.Unicode, Encoding.UTF32, Encoding.UTF7
                // using (StreamReader stream_reader = new StreamReader(fileStream, System.Text.Encoding.UTF8))
                if (null == i_file_encoding)
                {
                    reader = new StreamReader(responseStream, System.Text.Encoding.Default);
                }
                else
                {
                    reader = new StreamReader(responseStream, i_file_encoding);
                }

                file_content = reader.ReadToEnd();

                reader.Close();
                response.Close();
            }

            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

                o_error = ex.Message;

                return false;
            }

            File.WriteAllText(i_file_local, file_content);

            return true;

        } // GetFile

        /// <summary>Download of one file from a server directory to a local directory
        /// <para></para>
        /// <para></para>
        /// <para></para>
        /// <para></para>
        /// </summary>
        /// <param name="i_directory_server">The server directory e.g. /www/Audio/REG00011/AudioOne/</param>
        /// <param name="i_directory_local">The local directory e.g. C:\Apps\JazzLiveAarau\Admin\Dokumente</param>
        /// <param name="i_file_name">The file name</param>
        /// <param name="o_error">Error message</param>
        public bool DownloadFileServerLocal(string i_directory_server, string i_directory_local, string i_file_name, out string o_error)
        {
            o_error = "";

            string server_file_name = i_directory_server + i_file_name;
            string local_file_name = i_directory_local + @"\" + i_file_name;

            if (!DownloadFile(server_file_name, local_file_name, out o_error))
            {
                return false;
            }

            return true;

        } // DownloadFileServerLocal

        #endregion // Download one file

        #region Rename

        /// <summary>Rename a file
        /// <para>Not yet implemented </para>
        /// <para></para>
        /// </summary>
        /// <param name="i_file_server">The server file name e.g. /www/Audio/REG00011/AudioOne/Tune_1.mp3</param>
        /// <param name="i_file_server_new_name">The new name</param>
        /// <param name="o_error">Error message</param>
        public bool RenameFile(string i_file_server_name, string i_file_server_new_name, out string o_error)
        {
            o_error = "";


            /*
            var request = (FtpWebRequest)WebRequest.Create("ftp://www.mysite.com/test.jpg");
            request.Credentials = new NetworkCredential(user, pass);
            request.Method = WebRequestMethods.Ftp.Rename;
            request.RenameTo = "testrename.jpg"
            request.GetResponse().Close();

            FtpWebResponse resp = (FtpWebResponse)request.GetResponse();
            */

            return true;

        } // RenameFile

        #endregion // Rename

    } // DownLoad

    /// <summary>Internet utility functions</summary>
    public static class InternetUtil
    {
        /// <summary>Checks if an Internet server is available
        /// <para>This function does not always work. Better to use a function that tries to get data from the server</para>
        /// </summary>
        /// <param name="i_str_uri">Web address like for instance http://www.jazzliveaarau.ch </param>
        /// <param name="o_error">Error from Webresponse if there is no connection</param>
        /// <returns>Returns true if connection is available to the FTP host</returns>
        public static bool IsConnectionAvailable(string i_str_uri, out string o_error)
        {
            o_error = "";

            System.Uri objUrl = new System.Uri(i_str_uri);
            System.Net.WebRequest objWebReq;
            objWebReq = System.Net.WebRequest.Create(objUrl);
            System.Net.WebResponse objResp;
            try
            {
                objResp = objWebReq.GetResponse();
                objResp.Close();
                objWebReq = null;
                return true;
            }
            catch (Exception e)
            {
                objWebReq = null;

                o_error = e.Message;

                return false;
            } // catch
        } // IsConnectionAvailable

        /// <summary>Checks if an Internet server is available
        /// <para>This function does not always work. Better to use a function that tries to get data from the server</para>
        /// </summary>
        /// <param name="i_host">Web address like for instance www.jazzliveaarau.ch </param>
        /// <param name="o_error">Error from Webresponse if there is no connection</param>
        /// <returns>Returns true if connection is available to the FTP host</returns>
        public static bool IsConnectionAvailablePing(string i_host, out string o_status)
        {
            o_status = "";

            try
            {
                Ping host_ping = new Ping();
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions ping_options = new PingOptions();
                PingReply ping_reply = host_ping.Send(i_host, timeout, buffer, ping_options);
                if (ping_reply.Status == IPStatus.Success)
                {
                    // Pressumably online
                    o_status = ping_reply.Status.ToString();
                    return true;
                }

            }
            catch (Exception e)
            {
                o_status = "InternetUtil.IsConnectionAvailablePing No connection " + e.Message;
                return false;

            } // catch

            o_status = "InternetUtil.IsConnectionAvailablePing No connection (Programming error)";
            return false; // Should never come here

        } // IsConnectionAvailablePing

    } // InternetUtil



} // namespace
