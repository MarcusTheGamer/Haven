using Haven.Models;

namespace Haven.ViewModels;

public class ManageFamilyViewModel
{
    public List<RouteItem> Options { get; } =
    [
        new("Family Members", "FamilyMembersPage"),
        new("Family Settings", "FamilySettingsPage"),
        new("Invite Member", "InviteMemberPage"),
        new("Devices", "FamilyDevicesPage")
    ];
}