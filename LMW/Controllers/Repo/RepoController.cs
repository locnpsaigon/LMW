using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace LMW.Controllers.Repo
{
    public class RepoController : Controller
    {
        static string ROOT_PATH = ConfigurationManager.AppSettings["REPO_PATH"];

        // GET: Repo
        public ActionResult Index(string pathInfo = "/")
        {
            // validate input params
            pathInfo = pathInfo.Trim();
            if (pathInfo.Equals("Index") || String.IsNullOrWhiteSpace(pathInfo))
            {
                pathInfo = "/";
            }

            // get files/folers
            var currentPath = HostingEnvironment.MapPath(ROOT_PATH + "/" + pathInfo);
            var folders = Directory.GetDirectories(currentPath);
            var files = Directory.GetFiles(currentPath);

            // Gereate files/folders table
            var html = "";
            html += "<table>";
            html += "   <tbody>";
            html += "       <tr><th valign=\"top\"><img src=\"/Content/icons/blank.gif\" alt=\"[ICO]\"></th><th><a href=\"C=N;O=D\">Name</a></th><th><a href=\"C=M;O=A\">Last modified</a></th><th><a href=\"http://repo.kodi.vn/?C=S;O=A\">Size</a></th><th><a href=\"C=D;O=A\">Description</a></th></tr>";

            // Go parrent
            if (pathInfo.Equals("/") == false)
            {
                var temp = pathInfo.Split('/').ToList();
                temp.Remove(temp.LastOrDefault());
                html += "       <tr><td valign=\"top\"><img src=\"/Content/icons/back.gif\" alt=\"[PARENTDIR]\"></td><td><a href=\"/Repo?pathInfo=" + String.Join("/", temp) + "\">Parent Directory</a>       </td><td>&nbsp;</td><td align=\"right\">  - </td><td>&nbsp;</td></tr>";
            }

            // Generate folders list
            foreach (var folder in folders)
            {
                var di = new DirectoryInfo(folder);
                var href = (pathInfo.Equals("/") ? "" : pathInfo);
                href += href.EndsWith("/") ? di.Name : "/" + di.Name;
                html += "       <tr><td valign=\"top\"><img src=\"/Content/icons/folder.gif\" alt=\"[DIR]\"></td><td><a href=\"?pathInfo=" + href + "\">" + di.Name + "/</a>            </td><td align=\"right\">" + di.CreationTime.ToString("yyyy-MM-dd HH:mm") + "  </td><td align=\"right\">  - </td><td>&nbsp;</td></tr>";
            }

            // Generate files list
            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                var href = ROOT_PATH + (pathInfo.Equals("/") ? "" : pathInfo);
                href += href.EndsWith("/") ? fi.Name : "/" + fi.Name;
                html += "       <tr><td valign=\"top\"><img src=\"" + GetFileIcon(fi.Extension) + "\" alt=\"[   ]\"></td><td><a href=\"" + href + "\">" + fi.Name + "</a></td><td align=\"right\">" + fi.CreationTime.ToString("yyyy-MM-dd HH:mm") + "  </td><td align=\"right\"> " + GetFileSizeText(fi.Length) + "</td><td>&nbsp;</td></tr>";
            }

            html += "   </tbody>";
            html += "</table>";

            ViewBag.Path = pathInfo;
            ViewBag.FolderListTable = html;

            return View();
        }

        #region Utilities
        static string GetFileIcon(string extention)
        {
            switch (extention.ToLower())
            {
                case ".zip":
                    return "/Content/icons/compressed.gif";
                case ".py":
                    return "/Content/icons/p.gif";
                default:
                    return "/Content/icons/text.gif";
            }
        }

        static string GetFileSizeText(long size)
        {
            long gb = 1073741824;
            long mb = 1048576;
            long kb = 1024;
            // GB
            if (size > gb)
            {
                return (size / gb).ToString() + "G";
            }
            // MB
            else if (size > mb)
            {
                return (size / mb).ToString() + "M";
            }
            // KB
            else if (size > kb)
            {
                return (size / kb).ToString() + "K";
            }
            else
            {
                return size.ToString();
            }
        }
        #endregion
    }
}