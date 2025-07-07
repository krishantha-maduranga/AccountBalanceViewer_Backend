using AccountBalanceViewer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceViewer.Persistence.Configurations
{
    public class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
    {
        public void Configure(EntityTypeBuilder<AccountBalance> builder)
        {
            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Amount)
                .IsRequired();                

            builder.Property(e => e.Month)
                .IsRequired();

        }
    }
}
