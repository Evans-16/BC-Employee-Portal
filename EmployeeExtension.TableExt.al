tableextension 50100 "PortalEmployeeExt" extends Employee
{
    fields
    {
        // Field to store the hashed password for portal login
        field(50100; "Portal Password Hash"; Text[250])
        {
            Caption = 'Portal Password Hash';
            DataClassification = CustomerContent;
        }

        // Field to check if the employee's account has been verified/activated
        field(50101; "Portal Active"; Boolean)
        {
            Caption = 'Portal Active';
            DataClassification = CustomerContent;
        }

        // Optional field if you want them to map a secondary web-login email 
        field(50102; "Portal Personal Email"; Text[80])
        {
            Caption = 'Portal Personal Email';
            DataClassification = CustomerContent;
        }
    }
}