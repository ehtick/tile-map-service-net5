﻿using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace TileMapService.MBTiles
{
    /// <summary>
    /// Repository for MBTiles database access (in read only mode).
    /// </summary>
    /// <remarks>
    /// See https://github.com/mapbox/mbtiles-spec/blob/master/1.3/spec.md
    /// </remarks>
    class Repository
    {
        /// <summary>
        /// Connection string for SQLite database.
        /// </summary>
        private readonly string connectionString;

        #region MBTiles database objects names

        private const string TableTiles = "tiles";

        private const string ColumnTileColumn = "tile_column";

        private const string ColumnTileRow = "tile_row";

        private const string ColumnZoomLevel = "zoom_level";

        private const string ColumnTileData = "tile_data";

        private const string TableMetadata = "metadata";

        private const string ColumnMetadataName = "name";

        private const string ColumnMetadataValue = "value";

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Repository"/> class.
        /// </summary>
        /// <param name="path">Full path to MBTiles database file.</param>
        public Repository(string path)
        {
            this.connectionString = CreateSqliteConnectionString(path);
        }

        /// <summary>
        /// Reads tile image contents with given coordinates from database.
        /// </summary>
        /// <param name="tileColumn">Tile X coordinate (column).</param>
        /// <param name="tileRow">Tile Y coordinate (row), Y axis goes up from the bottom (TMS scheme).</param>
        /// <param name="zoomLevel">Tile Z coordinate (zoom level).</param>
        /// <seealso href="https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/async">Async Limitations</seealso>
        /// <returns>Tile image contents.</returns>
        public byte[] ReadTileData(int tileColumn, int tileRow, int zoomLevel)
        {
            var commandText = $"SELECT {ColumnTileData} FROM {TableTiles} WHERE (({ColumnZoomLevel} = @zoom_level) AND ({ColumnTileColumn} = @tile_column) AND ({ColumnTileRow} = @tile_row))";
            using (var connection = new SqliteConnection(this.connectionString))
            {
                using (var command = new SqliteCommand(commandText, connection))
                {
                    command.Parameters.AddRange(new[]
                    {
                        new SqliteParameter("@tile_column", tileColumn),
                        new SqliteParameter("@tile_row", tileRow),
                        new SqliteParameter("@zoom_level", zoomLevel),
                    });

                    connection.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        byte[] result = null;

                        if (dr.Read())
                        {
                            result = (byte[])dr[0];
                        }

                        dr.Close();

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Reads all metadata key/value items from database.
        /// </summary>
        /// <returns>Metadata records.</returns>
        public MetadataItem[] ReadMetadata()
        {
            using (var connection = new SqliteConnection(this.connectionString))
            {
                var commandText = $"SELECT {ColumnMetadataName}, {ColumnMetadataValue} FROM {TableMetadata}";
                using (var command = new SqliteCommand(commandText, connection))
                {
                    var result = new List<MetadataItem>();

                    connection.Open();
                    using (var dr = command.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new MetadataItem
                            {
                                Name = dr.IsDBNull(0) ? null : dr.GetString(0),
                                Value = dr.IsDBNull(1) ? null : dr.GetString(1),
                            });
                        }

                        dr.Close();
                    }

                    return result.ToArray();
                }
            }
        }

        /// <summary>
        /// Creates connection string for MBTiles database.
        /// </summary>
        /// <param name="source">Full path to MBTiles database file.</param>
        /// <returns>Connection string.</returns>
        private static string CreateSqliteConnectionString(string path)
        {
            return new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadOnly,
                Cache = SqliteCacheMode.Shared,
            }.ToString();
        }
    }
}