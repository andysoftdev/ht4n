/** -*- C++ -*-
* Copyright (C) 2010-2016 Thalmann Software & Consulting, http://www.softdev.ch
*
* This file is part of ht4n.
*
* ht4n is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 3
* of the License, or any later version.
*
* Hypertable is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
* 02110-1301, USA.
*/

#pragma once

#ifndef __cplusplus_cli
#error "requires /clr"
#endif

namespace Hypertable { namespace Xml {
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::ComponentModel;
	using namespace System::Xml;
	using namespace System::Xml::Serialization;

	ref class TableSchema;
	ref class AccessGroupOptions;
	ref class AccessGroup;
	ref class ColumnFamilyOptions;
	ref class ColumnFamily;

	/// <summary>
	/// Specifies whether the time order is ascending or descending.
	/// </summary>
	[XmlType(TypeName="timeOrderType", AnonymousType=true)]
	public enum class ColumnFamilyTimeOrder {

		/// <summary>
		/// Ascending time order.
		/// </summary>
		asc,

		/// <summary>
		/// Descending time order.
		/// </summary>
		desc,
	};
	
	/// <summary>
	/// Represents a access group options.
	/// </summary>
	[XmlType(AnonymousType=true)]
	public ref class AccessGroupOptions sealed {

		public:

			/// <summary>
			/// Gets or sets the replication level.
			/// </summary>
			/// <remarks>
			/// The replication option controls the replication level in the underlying distributed file system (DFS) for cell store files
			/// created for this access group. The default is unspecified, which translates to whatever the default replication level is
			/// for the underlying file system.
			/// </remarks>
			[XmlElement("Replication")]
			property int Replication;

			/// <summary>
			/// Gets or sets a value that indicates whether the Replication property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool ReplicationSpecified;

			/// <summary>
			/// Gets or sets the block size.
			/// </summary>
			/// <remarks>
			/// he block size option controls the size of the compressed blocks in the cell stores. A smaller block
			/// size minimizes the amount of data that must be read from disk and decompressed for a key lookup at the
			/// expense of a larger block index which consumes memory. The default value for the block size is 65KB.
			/// </remarks>
			[XmlElement("BlockSize")]
			property int BlockSize;

			/// <summary>
			/// Gets or sets a value that indicates whether the BlockSize property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool BlockSizeSpecified;

			/// <summary>
			/// Gets or sets the compressor for the access group.
			/// </summary>
			/// <remarks>
			/// The cell store blocks within an access group are compressed using the compression codec that is specified for the access group.
			/// The following compression codecs are available: bmz, lzo, quicklz, snappy, zlib, none
			/// </remarks>
			[XmlElement("Compressor")]
			property String^ Compressor;

			/// <summary>
			/// Gets or sets the bloom filter specification.
			/// </summary>
			[XmlElement("BloomFilter")]
			property String^ BloomFilter;

			/// <summary>
			/// Gets or sets a value that indicates whether the AccessGroup should remain memory resident.
			/// </summary>
			[XmlElement("InMemory")]
			property bool InMemory;

			/// <summary>
			/// Gets or sets a value that indicates whether the InMemory property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool InMemorySpecified;
	};

	/// <summary>
	/// Represents the column family options.
	/// </summary>
	[XmlType(AnonymousType=true)]
	public ref class ColumnFamilyOptions sealed {

		public:

			/// <summary>
			/// Gets or sets a value that defines the N most recent versions of each cell to keep.
			/// </summary>
			[XmlElement("MaxVersions")]
			property int MaxVersions;

			/// <summary>
			/// Gets or sets a value that indicates whether the MaxVersions property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool MaxVersionsSpecified;

			/// <summary>
			/// Gets or sets the time order.
			/// </summary>
			[XmlElement("TimeOrder")]
			property ColumnFamilyTimeOrder TimeOrder;

			/// <summary>
			/// Gets or sets a value that indicates whether the TimeOrder property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool TimeOrderSpecified;

			/// <summary>
			/// Gets or sets a value that specify to keep cell versions that fall within some time window [ms] in the immediate past.
			/// </summary>
			[XmlElement("TTL")]
			property int Ttl;

			/// <summary>
			/// Gets or sets a value that indicates whether the Ttl property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool TtlSpecified;

			/// <summary>
			/// Gets or sets a value that indicates whether this column family is a counter column.
			/// </summary>
			[XmlElement("Counter")]
			property bool Counter;

			/// <summary>
			/// Gets or sets a value that indicates whether the Counter property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool CounterSpecified;
	};

	/// <summary>
	/// Represents a Hypertable xml table schema.
	/// </summary>
	/// <example>
	/// The following example shows how to parse a xml schema string.
	/// <code>
	/// string schema = 
	///    "&lt;Schema&gt;"
	/// +     "&lt;AccessGroup name=\"default\"&gt;"
	/// +     "&lt;ColumnFamily&gt;"
	/// +         "&lt;Name&gt;cf&lt;/Name&gt;"
	/// +     "&lt;/ColumnFamily&gt;"
	/// +     "&lt;/AccessGroup&gt;"
	/// + "&lt;/Schema&gt;";
	///
	/// var tableSchema = TableSchema.Parse(schema);
	/// </code>
	/// The following example shows how to render a xml schema string.
	/// <code>
	/// var accessGroup = new AccessGroup { Name = "ag", ColumnFamilies = new List&lt;ColumnFamily&gt;() };
	/// accessGroup.ColumnFamilies.Add(new ColumnFamily { Name = "cfa" });
	/// accessGroup.ColumnFamilies.Add(new ColumnFamily { Name = "cfb", MaxVersions = 5, MaxVersionsSpecified = true });
	///
	/// var schema = new TableSchema { AccessGroups = new List&lt;AccessGroup&gt;() };
	/// schema.AccessGroups.Add(accessGroup);
	///
	/// var xml = schema.ToString();
	/// </code>
	/// </example>
	[XmlRoot(ElementName="Schema", IsNullable=false), XmlType(AnonymousType=true)]
	public ref class TableSchema sealed {

		public:

			/// <summary>
			/// Gets or sets the column family generation.
			/// </summary>
			[XmlElement("Generation")]
			property UInt64 Generation;

			/// <summary>
			/// Gets or sets a value that indicates whether the Generation property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool GenerationSpecified;

			/// <summary>
			/// Gets or sets the group commit interval.
			/// </summary>
			/// <remarks>
			/// The group commit interval option tells the system that updates to this table should be carried out with group
			/// commit and also specifies the commit interval in milliseconds.
			/// </remarks>
			[XmlElement("GroupCommitInterval")]
			property int GroupCommitInterval;

			/// <summary>
			/// Gets or sets a value that indicates whether the GroupCommitInterval property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool GroupCommitIntervalSpecified;

			/// <summary>
			/// Gets or sets the access group defaults.
			/// </summary>
			[XmlElement("AccessGroupDefaults")]
			property AccessGroupOptions^ AccessGroupDefaults;

			/// <summary>
			/// Gets or sets the column family defaults.
			/// </summary>
			[XmlElement("ColumnFamilyDefaults")]
			property ColumnFamilyOptions^ ColumnFamilyDefaults;

			/// <summary>
			/// Gets or sets the access groups.
			/// </summary>
			[XmlElement(L"AccessGroup")]
			property List<AccessGroup^>^ AccessGroups;

			/// <summary>
			/// Initializes a new instance of the TableSchema class.
			/// </summary>
			TableSchema( ) {
				AccessGroups = gcnew List<AccessGroup^>();
			}

			/// <summary>
			/// Converts the xml table schema representation to a TableSchema object.
			/// </summary>
			/// <param name="xml">The xml table schema.</param>
			/// <returns>
			/// The table schema object or null.
			/// </returns>
			static TableSchema^ Parse( String^ xml );

			/// <summary>
			/// Converts the table schema to it's xml representation.
			/// </summary>
			/// <returns>
			/// The xml table schema representation.
			/// </returns>
			virtual String^ ToString( ) override;
	};

	/// <summary>
	/// Represents a access group. Access groups provide control over the physical layout of the table data on disk.
	/// The data for all column families in the same access group are stored physically together on disk.
	/// </summary>
	[XmlType(AnonymousType=true)]
	public ref class AccessGroup sealed {

		public:

			/// <summary>
			/// Gets or sets the access group name.
			/// </summary>
			/// <remarks>
			/// Access group names must be unique for the entire table schema.
			/// </remarks>
			[XmlAttribute("name")]
			property String^ Name;

			/// <summary>
			/// Gets or sets the column family generation.
			/// </summary>
			[XmlElement("Generation")]
			property UInt64 Generation;

			/// <summary>
			/// Gets or sets a value that indicates whether the Generation property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool GenerationSpecified;

			/// <summary>
			/// Gets or sets the access group options.
			/// </summary>
			[XmlElement("Options")]
			property AccessGroupOptions^ Options;

			/// <summary>
			/// Gets or sets the column family defaults.
			/// </summary>
			[XmlElement("ColumnFamilyDefaults")]
			property ColumnFamilyOptions^ ColumnFamilyDefaults;

			/// <summary>
			/// Gets or sets the column families.
			/// </summary>
			[XmlElement(L"ColumnFamily")]
			property List<ColumnFamily^>^ ColumnFamilies;

			/// <summary>
			/// Initializes a new instance of the AccessGroup class.
			/// </summary>
			AccessGroup( ) {
				ColumnFamilies = gcnew List<ColumnFamily^>();
			}
	};

	/// <summary>
	/// Represents a column family.
	/// </summary>
	[XmlType(AnonymousType=true)]
	public ref class ColumnFamily sealed {

		public:

			/// <summary>
			/// Gets or sets the column family identifier.
			/// </summary>
			[XmlAttribute("id")]
			property int Id;

			/// <summary>
			/// Gets or sets a value that indicates whether the Id property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool IdSpecified;

			/// <summary>
			/// Gets or sets the column family generation.
			/// </summary>
			[XmlElement("Generation")]
			property UInt64 Generation;

			/// <summary>
			/// Gets or sets a value that indicates whether the Generation property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool GenerationSpecified;

			/// <summary>
			/// Gets or sets the mandatory column family name.
			/// </summary>
			/// <remarks>
			/// Column family names must be unique for the entire table schema.
			/// </remarks>
			[XmlElement("Name")]
			property String^ Name;

			/// <summary>
			/// Gets or sets a value that indicates whether the column family has been deleted.
			/// </summary>
			[XmlElement("Deleted")]
			property bool Deleted;

			/// <summary>
			/// Gets or sets a value that indicates whether the Deleted property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool DeletedSpecified;

			/// <summary>
			/// Gets or sets a value that indicates whether the column family has a cell value index.
			/// </summary>
			[XmlElement("Index")]
			property bool Index;

			/// <summary>
			/// Gets or sets a value that indicates whether the Index property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool IndexSpecified;

			/// <summary>
			/// Gets or sets a value that indicates whether the column family has a column qualifier index.
			/// </summary>
			[XmlElement("QualifierIndex")]
			property bool QualifierIndex;

			/// <summary>
			/// Gets or sets a value that indicates whether the QualifierIndex property has been specified.
			/// </summary>
			[XmlIgnore]
			property bool QualifierIndexSpecified;

			/// <summary>
			/// Gets or sets the column family options.
			/// </summary>
			[XmlElement("Options")]
			property ColumnFamilyOptions^ Options;

			/// <summary>
			/// Initializes a new instance of the ColumnFamily class.
			/// </summary>
			ColumnFamily( ) { }

	};

} }
