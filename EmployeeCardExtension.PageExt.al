pageextension 50100 "PortalEmployeeCardExt" extends "Employee Card"
{
    layout
    {
        // Insert fields at the bottom of the standard "General" group on the Employee Card
        addlast(General)
        {
            field("Portal Password Hash"; Rec."Portal Password Hash")
            {
                ApplicationArea = All;
                ToolTip = 'Specifies the encrypted password used for the web portal.';
            }
            field("Portal Active"; Rec."Portal Active")
            {
                ApplicationArea = All;
                ToolTip = 'Specifies if the employee is allowed to log into the web portal.';
            }
            field("Portal Personal Email"; Rec."Portal Personal Email")
            {
                ApplicationArea = All;
                ToolTip = 'Specifies the email address used explicitly for portal credentials.';
            }
        }
    }
}