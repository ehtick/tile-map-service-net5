﻿namespace TileMapService
{
    /// <summary>
    /// Media type identifiers.
    /// </summary>
    /// <remarks>
    /// Structure is similar to <see cref="System.Net.Mime.MediaTypeNames"/> class.
    /// </remarks>
    internal static class MediaTypeNames
    {
        public static class Image
        {
            public const string Png = "image/png";

            public const string Jpeg = "image/jpeg";
        }

        public static class Text
        {
            public const string Xml = "text/xml";
        }

        public static class Application
        {
            public const string MapboxVectorTile = "application/vnd.mapbox-vector-tile";
        
            public const string XProtobuf = "application/x-protobuf"; 
        }
    }
}
