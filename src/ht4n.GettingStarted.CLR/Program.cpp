/** -*- C++ -*-
 * Copyright (C) 2010-2015 Thalmann Software & Consulting, http://www.softdev.ch
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

#include "stdafx.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Globalization;
using namespace System::Text;

using namespace Hypertable;

int main(array<String^> ^args)
{
    // Preamble
    Console::WriteLine("Welcome to ht4n getting started C++/CLR.");
    Console::WriteLine("For more information about ht4n, visit http://ht4n.softdev.ch/");
    Console::WriteLine();

    try {

        // Compose the connection string - Provider={0};Uri={1}
        String^ connectionString = String::Format(
            CultureInfo::InvariantCulture,
            "Provider={0};Uri={1}",
            args->Length > 0 ? args[0] : "Hyper",
            args->Length > 1 ? args[1] : "net.tcp://localhost");

        // Connect to the database instance
        Console::WriteLine("Connecting {0}", connectionString);
        IContext^ context = nullptr;
        IClient^ client = nullptr;
        INamespace^ ns = nullptr;
        ITable^ table = nullptr;
        try {
            context = Context::Create(connectionString);
            client = context->CreateClient();

            // Open or create namespace
            Console::WriteLine("Open or create namespace 'tutorials'");
            ns = client->OpenNamespace("tutorials", OpenDispositions::OpenAlways | OpenDispositions::CreateIntermediate);

            // Define the table schema using xml
            String^ TableSchema =
                "<Schema>" +
                "<AccessGroup name=\"default\">" +
                "<ColumnFamily><Name>color</Name></ColumnFamily>" +
                "<ColumnFamily><Name>energy</Name></ColumnFamily>" +
                "<ColumnFamily><Name>protein</Name></ColumnFamily>" +
                "<ColumnFamily><Name>vitamins</Name></ColumnFamily>" +
                "</AccessGroup>" +
                "</Schema>";

            // Open or create table
            Console::WriteLine("Open or create table 'fruits'");
            table = ns->OpenTable("fruits", TableSchema, OpenDispositions::OpenAlways);

            // Insert some fruits
            ITableMutator^ mutator = nullptr;
            try {
                mutator = table->CreateMutator();

                Console::WriteLine("Insert 'apple' into 'fruits'");
                Key^ key = gcnew Key();
                key->Row = "apple";
                key->ColumnFamily = "color";
                mutator->Set(key, Encoding::UTF8->GetBytes("red"));

                key->ColumnFamily = "energy";
                mutator->Set(key, BitConverter::GetBytes(207)); // [KJ]

                key->ColumnFamily = "protein";
                mutator->Set(key, BitConverter::GetBytes(0.4)); // [g]

                key->ColumnFamily = "vitamins";
                key->ColumnQualifier = "C";
                mutator->Set(key, BitConverter::GetBytes(15.0)); // [mg]

                key->ColumnQualifier = "B1";
                mutator->Set(key, BitConverter::GetBytes(0.02)); // [mg]

                Console::WriteLine("Insert 'banana' into 'fruits'");
                key = gcnew Key();
                key->Row = "banana";
                key->ColumnFamily = "color";
                mutator->Set(key, Encoding::UTF8->GetBytes("yellow"));

                key->ColumnFamily = "energy";
                mutator->Set(key, BitConverter::GetBytes(375)); // [KJ]

                key->ColumnFamily = "protein";
                mutator->Set(key, BitConverter::GetBytes(1.2)); // [g]

                key->ColumnFamily = "vitamins";
                key->ColumnQualifier = "C";
                mutator->Set(key, BitConverter::GetBytes(10.0)); // [mg]

                key->ColumnQualifier = "B1";
                mutator->Set(key, BitConverter::GetBytes(0.04)); // [mg]
            }
            finally {
              if (mutator != nullptr) {
                  delete mutator;
                  mutator = nullptr;
              }
            }

            Console::WriteLine();

            // Some query examples
            Console::WriteLine("Select all cells from 'fruits'");
            ITableScanner^ scanner = nullptr;
            try {
                scanner = table->CreateScanner();
                for each (Cell^ cell in scanner) {
                    Console::WriteLine(cell);
                }
            }
            finally {
              if (scanner != nullptr) {
                  delete scanner;
                  scanner = nullptr;
              }
            }

            Console::WriteLine();

            Console::WriteLine("Select all 'apple' from 'fruits'");
            ScanSpec^ scanSpec = gcnew ScanSpec("apple");
            try {
                scanner = table->CreateScanner(scanSpec);
                Cell^ cell;
                while (scanner->Next(cell)) {
                    Console::WriteLine(cell);
                }
            }
            finally {
              if (scanner != nullptr) {
                  delete scanner;
                  scanner = nullptr;
              }
            }

            Console::WriteLine();

            Console::WriteLine("Select all 'vitamins' for the 'banana'");
            scanSpec = gcnew ScanSpec("banana");
            scanSpec->AddColumn("vitamins");
            try {
                scanner = table->CreateScanner(scanSpec);
                Cell^ cell = gcnew Cell();
                while (scanner->Move(cell)) { // caution, re-use cell instance
                    Console::WriteLine("{0} {1} {2}mg", cell->Key->Row, cell->Key->Column, BitConverter::ToDouble(cell->Value, 0));
                }
            }
            finally {
              if (scanner != nullptr) {
                  delete scanner;
                  scanner = nullptr;
              }
            }

            Console::WriteLine();

            // Drop table
            Console::WriteLine("Drop table 'fruits'");
            ns->DropTable("fruits");
        }
        finally {
          if (table != nullptr) {
              delete table;
          }
          if (ns != nullptr) {
              delete ns;
          }
          if (client != nullptr) {
              delete client;
          }
          if (context != nullptr) {
              delete context;
          }
        }
    }
    catch (Exception^ e) {
        Console::WriteLine();
        Console::WriteLine(e);
    }

    return 0;
}
