using System.Text.Json.Serialization;

namespace TenureInformationApi.V1.Domain
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HouseholdMembersType
    {
        Person,
        Organisation
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TenuredAssetType
    {
        Block,
        Concierge,
        Dwelling,
        LettableNonDwelling,
        MediumRiseBlock,
        NA,
        TravellerSite
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PersonTenureType
    {
        Tenant,
        Leaseholder,
        Freeholder,
        HouseholdMember,
        Occupant
    }
}