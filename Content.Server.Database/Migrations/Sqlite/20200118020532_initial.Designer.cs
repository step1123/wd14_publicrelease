﻿// <auto-generated />

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Server.Database.Migrations.Sqlite
{
    [DbContext(typeof(SqlitePreferencesDbContext))]
    [Migration("20200118020532_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.0");

            modelBuilder.Entity("Content.Server.Database.HumanoidProfile", b =>
                {
                    b.Property<int>("HumanoidProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Age")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CharacterName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EyeColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FacialHairColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FacialHairName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HairColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("HairName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PrefsId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Sex")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SkinColor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Slot")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SlotName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("HumanoidProfileId");

                    b.HasIndex("PrefsId");

                    b.ToTable("HumanoidProfile");
                });

            modelBuilder.Entity("Content.Server.Database.Prefs", b =>
                {
                    b.Property<int>("PrefsId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("SelectedCharacterSlot")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("PrefsId");

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Preferences");
                });

            modelBuilder.Entity("Content.Server.Database.HumanoidProfile", b =>
                {
                    b.HasOne("Content.Server.Database.Prefs", "Prefs")
                        .WithMany("HumanoidProfiles")
                        .HasForeignKey("PrefsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
