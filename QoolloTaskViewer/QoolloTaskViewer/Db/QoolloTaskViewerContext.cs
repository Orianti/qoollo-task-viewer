﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QoolloTaskViewer.Models;

namespace QoolloTaskViewer.Db
{
    public class QoolloTaskViewerContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ServiceModel> Services { get; set; }
        public DbSet<TokenModel> Tokens { get; set; }

        public QoolloTaskViewerContext()
        {
            Database.EnsureCreated();
        }

        public QoolloTaskViewerContext(DbContextOptions<QoolloTaskViewerContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}