using System;
using System.Collections.Generic;

namespace RestServer.Helper {
    /// <summary>
    /// Dictionary of file extensions and their corresponding mime types.
    /// </summary>
    public class MimeTypeDict {
        public static IDictionary<string, string> MIME_TYPE_MAPPINGS =
            new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
                { "asf", "video/x-ms-asf" },
                { "asx", "video/x-ms-asf" },
                { "avi", "video/x-msvideo" },
                { "bin", "application/octet-stream" },
                { "cco", "application/x-cocoa" },
                { "crt", "application/x-x509-ca-cert" },
                { "css", "text/css" },
                { "deb", "application/octet-stream" },
                { "der", "application/x-x509-ca-cert" },
                { "dll", "application/octet-stream" },
                { "dmg", "application/octet-stream" },
                { "ear", "application/java-archive" },
                { "eot", "application/octet-stream" },
                { "exe", "application/octet-stream" },
                { "flv", "video/x-flv" },
                { "gif", "image/gif" },
                { "hqx", "application/mac-binhex40" },
                { "htc", "text/x-component" },
                { "htm", "text/html" },
                { "html", "text/html" },
                { "ico", "image/x-icon" },
                { "img", "application/octet-stream" },
                { "iso", "application/octet-stream" },
                { "jar", "application/java-archive" },
                { "jardiff", "application/x-java-archive-diff" },
                { "jng", "image/x-jng" },
                { "jnlp", "application/x-java-jnlp-file" },
                { "jpeg", "image/jpeg" },
                { "jpg", "image/jpeg" },
                { "js", "application/javascript" },
                { "json", "application/json" },
                { "mml", "text/mathml" },
                { "mng", "video/x-mng" },
                { "mov", "video/quicktime" },
                { "mp3", "audio/mpeg" },
                { "mp4", "video/mp4" },
                { "mpeg", "video/mpeg" },
                { "mpg", "video/mpeg" },
                { "msi", "application/octet-stream" },
                { "msm", "application/octet-stream" },
                { "msp", "application/octet-stream" },
                { "pdb", "application/x-pilot" },
                { "pdf", "application/pdf" },
                { "pem", "application/x-x509-ca-cert" },
                { "pl", "application/x-perl" },
                { "pm", "application/x-perl" },
                { "png", "image/png" },
                { "prc", "application/x-pilot" },
                { "ra", "audio/x-realaudio" },
                { "rar", "application/x-rar-compressed" },
                { "rpm", "application/x-redhat-package-manager" },
                { "rtf", "application/rtf" },
                { "rss", "text/xml" },
                { "run", "application/x-makeself" },
                { "sea", "application/x-sea" },
                { "shtml", "text/html" },
                { "sit", "application/x-stuffit" },
                { "svg", "image/svg+xml" },
                { "swf", "application/x-shockwave-flash" },
                { "tcl", "application/x-tcl" },
                { "tk", "application/x-tcl" },
                { "txt", "text/plain" },
                { "tiff", "image/tiff" },
                { "csv", "text/csv" },
                { "war", "application/java-archive" },
                { "wbmp", "image/vnd.wap.wbmp" },
                { "wmv", "video/x-ms-wmv" },
                { "xml", "text/xml" },
                { "xpi", "application/x-xpinstall" },
                { "zip", "application/zip" },
            };

        public static bool IsBinary(string mimeType) {
            return !mimeType.StartsWith("text/");
        }
    }
}