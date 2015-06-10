﻿namespace Dapper.FastCrud.Mappings
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class AutoGeneratedEntityMapping<TEntity>:EntityMapping<TEntity>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AutoGeneratedEntityMapping()
        {
            var tableDescriptor = TypeDescriptor.GetAttributes(typeof(TEntity)).OfType<TableAttribute>().SingleOrDefault();
            if (tableDescriptor != null)
            {
                if (!string.IsNullOrEmpty(tableDescriptor.Name))
                {
                    this.SetTableName(tableDescriptor.Name);
                }
                if (!string.IsNullOrEmpty(tableDescriptor.Schema))
                {
                    this.SetSchemaName(tableDescriptor.Schema);
                }
            }
            this.DiscoverPropertyMappings();
        }

        private static readonly Type[] SimpleSqlTypes = new[]
        {
            typeof (byte),
            typeof (sbyte),
            typeof (short),
            typeof (ushort),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong),
            typeof (float),
            typeof (double),
            typeof (decimal),
            typeof (bool),
            typeof (string),
            typeof (char),
            typeof (Guid),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (byte[])
        };

        private static bool IsSimpleSqlType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            type = underlyingType ?? type;
            return type.IsEnum || SimpleSqlTypes.Contains(type);
        }

        private void DiscoverPropertyMappings()
        {
            foreach (var property in TypeDescriptor.GetProperties(EntityType).OfType<PropertyDescriptor>())
            {
                ForeignKeyAttribute foreignKey = null;
                if (IsSimpleSqlType(property.PropertyType)
                    && property.Attributes.OfType<EditableAttribute>().All(editableAttr => editableAttr.AllowEdit))
                {
                    var propertyMappingOptions = PropertyMappingOptions.None;
                    if (property.Attributes.OfType<KeyAttribute>().Any())
                    {
                        propertyMappingOptions |= PropertyMappingOptions.KeyProperty;
                    }

                    if (property.Attributes.OfType<DatabaseGeneratedAttribute>()
                                .Any(dbGenerated => dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed))
                    {
                        propertyMappingOptions |= PropertyMappingOptions.DatabaseGeneratedProperty;
                        propertyMappingOptions |= PropertyMappingOptions.ExcludedFromInserts;
                    }

                    if (property.Attributes.OfType<DatabaseGeneratedAttribute>()
                                .Any(dbGenerated => dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
                    {
                        propertyMappingOptions |= PropertyMappingOptions.DatabaseGeneratedProperty;
                        propertyMappingOptions |= PropertyMappingOptions.ExcludedFromInserts;
                        propertyMappingOptions |= PropertyMappingOptions.ExcludedFromUpdates;
                    }

                    var databaseColumnName = property.Attributes.OfType<ColumnAttribute>().FirstOrDefault()?.Name;
                    this.SetPropertyInternal(property, propertyMappingOptions, databaseColumnName);
                }
                else if((foreignKey = property.Attributes.OfType<ForeignKeyAttribute>().SingleOrDefault())!= null)
                {
                    this.SetPropertyInternal(property, PropertyMappingOptions.None, null, foreignKey.Name);
                }
            }
        }

        // OBSOLETE
        //private void DiscoverProperties()
        //{
        //    this.SelectPropertyDescriptors =
        //        TypeDescriptor.GetProperties(entityType)
        //            .Cast<PropertyDescriptor>()
        //            .Where(
        //                p =>
        //                    IsSimpleSqlType(p.PropertyType)
        //                    && p.Attributes.OfType<EditableAttribute>().All(editableAttr => editableAttr.AllowEdit))
        //            .ToArray();
        //    this.KeyPropertyDescriptors = this.SelectPropertyDescriptors.Where(propInfo => propInfo.Attributes.OfType<KeyAttribute>().Any()).ToArray();
        //    this.TableDescriptor = TypeDescriptor.GetAttributes(entityType)
        //        .OfType<TableAttribute>().SingleOrDefault() ?? new TableAttribute(entityType.Name);
        //    this.DatabaseGeneratedIdentityPropertyDescriptors = this.SelectPropertyDescriptors
        //        .Where(propInfo => propInfo.Attributes.OfType<DatabaseGeneratedAttribute>()
        //        .Any(dbGenerated => dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
        //        .ToArray();
        //    this.DatabaseGeneratedPropertyDescriptors = this.SelectPropertyDescriptors
        //        .Where(propInfo => propInfo.Attributes.OfType<DatabaseGeneratedAttribute>()
        //        .Any(dbGenerated => dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed || dbGenerated.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity))
        //        .ToArray();

        //    // everything can be updateable, with the exception of the primary keys
        //    this.UpdatePropertyDescriptors = this.SelectPropertyDescriptors.Except(this.KeyPropertyDescriptors).ToArray();

        //    // we consider properties that go into an insert only the ones that are not auto-generated 
        //    this.InsertPropertyDescriptors = this.SelectPropertyDescriptors.Except(this.DatabaseGeneratedPropertyDescriptors).ToArray();

        //}
    }
}