using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiGMPKlik.Migrations
{
    /// <inheritdoc />
    public partial class AddWilayah : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Wilayah_Provinsi",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kode_provinsi = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    nama = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    timezone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_Provinsi", x => x.Id);
                    table.CheckConstraint("CK_Provinsi_KodeLength", "LEN([kode_provinsi]) = 2");
                    table.CheckConstraint("CK_Provinsi_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                    table.CheckConstraint("CK_Provinsi_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
                });

            migrationBuilder.CreateTable(
                name: "Wilayah_KotaKab",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    provinsi_id = table.Column<int>(type: "int", nullable: false),
                    kode_kota_kabupaten = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    nama = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    jenis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_KotaKab", x => x.Id);
                    table.CheckConstraint("CK_KotaKab_Jenis", "[jenis] IN ('Kabupaten', 'Kota')");
                    table.CheckConstraint("CK_KotaKab_KodeLength", "LEN([kode_kota_kabupaten]) = 4");
                    table.CheckConstraint("CK_KotaKab_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                    table.CheckConstraint("CK_KotaKab_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
                    table.ForeignKey(
                        name: "FK_Wilayah_KotaKab_Wilayah_Provinsi_provinsi_id",
                        column: x => x.provinsi_id,
                        principalSchema: "dbo",
                        principalTable: "Wilayah_Provinsi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wilayah_Kecamatan",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kota_kabupaten_id = table.Column<int>(type: "int", nullable: false),
                    kode_kecamatan = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    nama = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_Kecamatan", x => x.Id);
                    table.CheckConstraint("CK_Kecamatan_KodeLength", "LEN([kode_kecamatan]) = 7");
                    table.CheckConstraint("CK_Kecamatan_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                    table.CheckConstraint("CK_Kecamatan_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
                    table.ForeignKey(
                        name: "FK_Wilayah_Kecamatan_Wilayah_KotaKab_kota_kabupaten_id",
                        column: x => x.kota_kabupaten_id,
                        principalSchema: "dbo",
                        principalTable: "Wilayah_KotaKab",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wilayah_KelurahanDesa",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kecamatan_id = table.Column<int>(type: "int", nullable: false),
                    kode_kelurahan_desa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    nama = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    jenis = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    kode_pos = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(10,8)", precision: 10, scale: 8, nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(11,8)", precision: 11, scale: 8, nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_KelurahanDesa", x => x.Id);
                    table.CheckConstraint("CK_KelurahanDesa_Jenis", "[jenis] IN ('Desa', 'Kelurahan')");
                    table.CheckConstraint("CK_KelurahanDesa_KodeLength", "LEN([kode_kelurahan_desa]) = 10");
                    table.CheckConstraint("CK_KelurahanDesa_KodePos", "[kode_pos] IS NULL OR LEN([kode_pos]) = 5");
                    table.CheckConstraint("CK_KelurahanDesa_Latitude", "[latitude] >= -90 AND [latitude] <= 90");
                    table.CheckConstraint("CK_KelurahanDesa_Longitude", "[longitude] >= -180 AND [longitude] <= 180");
                    table.ForeignKey(
                        name: "FK_Wilayah_KelurahanDesa_Wilayah_Kecamatan_kecamatan_id",
                        column: x => x.kecamatan_id,
                        principalSchema: "dbo",
                        principalTable: "Wilayah_Kecamatan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wilayah_Dusun",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kelurahan_desa_id = table.Column<int>(type: "int", nullable: false),
                    nama = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_Dusun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wilayah_Dusun_Wilayah_KelurahanDesa_kelurahan_desa_id",
                        column: x => x.kelurahan_desa_id,
                        principalSchema: "dbo",
                        principalTable: "Wilayah_KelurahanDesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wilayah_RW",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dusun_id = table.Column<int>(type: "int", nullable: false),
                    nama = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_RW", x => x.Id);
                    table.CheckConstraint("CK_Rw_Nama_Numeric", "ISNUMERIC([nama]) = 1");
                    table.CheckConstraint("CK_Rw_Nama_Range", "CONVERT(int, [nama]) >= 1 AND CONVERT(int, [nama]) <= 999");
                    table.ForeignKey(
                        name: "FK_Wilayah_RW_Wilayah_Dusun_dusun_id",
                        column: x => x.dusun_id,
                        principalSchema: "dbo",
                        principalTable: "Wilayah_Dusun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wilayah_RT",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rw_id = table.Column<int>(type: "int", nullable: false),
                    nama = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wilayah_RT", x => x.Id);
                    table.CheckConstraint("CK_Rt_Nama_Numeric", "ISNUMERIC([nama]) = 1");
                    table.CheckConstraint("CK_Rt_Nama_Range", "CONVERT(int, [nama]) >= 1 AND CONVERT(int, [nama]) <= 999");
                    table.ForeignKey(
                        name: "FK_Wilayah_RT_Wilayah_RW_rw_id",
                        column: x => x.rw_id,
                        principalSchema: "dbo",
                        principalTable: "Wilayah_RW",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_Dusun_kelurahan_desa_id_nama",
                schema: "dbo",
                table: "Wilayah_Dusun",
                columns: new[] { "kelurahan_desa_id", "nama" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_Kecamatan_kode_kecamatan",
                schema: "dbo",
                table: "Wilayah_Kecamatan",
                column: "kode_kecamatan",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_Kecamatan_kota_kabupaten_id_nama",
                schema: "dbo",
                table: "Wilayah_Kecamatan",
                columns: new[] { "kota_kabupaten_id", "nama" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_KelurahanDesa_kecamatan_id_nama",
                schema: "dbo",
                table: "Wilayah_KelurahanDesa",
                columns: new[] { "kecamatan_id", "nama" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_KelurahanDesa_kode_kelurahan_desa",
                schema: "dbo",
                table: "Wilayah_KelurahanDesa",
                column: "kode_kelurahan_desa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_KotaKab_kode_kota_kabupaten",
                schema: "dbo",
                table: "Wilayah_KotaKab",
                column: "kode_kota_kabupaten",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_KotaKab_provinsi_id_nama",
                schema: "dbo",
                table: "Wilayah_KotaKab",
                columns: new[] { "provinsi_id", "nama" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_Provinsi_kode_provinsi",
                schema: "dbo",
                table: "Wilayah_Provinsi",
                column: "kode_provinsi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_Provinsi_nama",
                schema: "dbo",
                table: "Wilayah_Provinsi",
                column: "nama",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_RT_rw_id_nama",
                schema: "dbo",
                table: "Wilayah_RT",
                columns: new[] { "rw_id", "nama" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wilayah_RW_dusun_id_nama",
                schema: "dbo",
                table: "Wilayah_RW",
                columns: new[] { "dusun_id", "nama" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wilayah_RT",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wilayah_RW",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wilayah_Dusun",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wilayah_KelurahanDesa",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wilayah_Kecamatan",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wilayah_KotaKab",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Wilayah_Provinsi",
                schema: "dbo");
        }
    }
}
