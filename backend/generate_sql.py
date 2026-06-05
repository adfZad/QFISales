import pandas as pd
import sys

excel_path = r'C:\Users\ADMIN\Downloads\Masterial & Customer Master (QFI) (1).xlsx'
sql_path = 'update_material_group.sql'

try:
    df = pd.read_excel(excel_path, sheet_name=0)
    
    # We need the 'Material' column for matching and the 'Material Group' column for the new data
    if 'Material' not in df.columns or 'Material Group' not in df.columns:
        print("Required columns not found in Excel")
        sys.exit(1)
        
    df = df.dropna(subset=['Material', 'Material Group'])
    
    with open(sql_path, 'w') as f:
        # Add the new column. Let's call it Material_Group_Name to avoid conflict with Material_Group
        f.write("USE [QFISales];\n")
        f.write("GO\n")
        f.write("IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Material_Group_Name' AND Object_ID = Object_ID(N'dbo.Material Master'))\n")
        f.write("BEGIN\n")
        f.write("    ALTER TABLE [dbo].[Material Master] ADD [Material_Group_Name] VARCHAR(255);\n")
        f.write("END\n")
        f.write("GO\n\n")
        
        # Generate UPDATE statements
        for index, row in df.iterrows():
            mat_code = str(row['Material']).strip().replace("'", "''")
            mat_group_name = str(row['Material Group']).strip().replace("'", "''")
            
            f.write(f"UPDATE [dbo].[Material Master] SET [Material_Group_Name] = '{mat_group_name}' WHERE [Material] = '{mat_code}';\n")
            
    print(f"SQL script successfully generated at {sql_path}")
except Exception as e:
    print(f"Error: {e}")
