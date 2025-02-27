using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class CompanySell : BaseEntity
{
    public Guid CompanyId { get; private set; }
    public Guid CharacterId { get; private set; }
    public Guid ItemTemplateId { get; private set; }
    public int Quantity { get; private set; }
    public int CostPrice﻿ { get; private set; }
    public int SellPrice﻿ { get; private set; }
    public Guid? SerialNumber { get; private set; }

    [JsonIgnore]
    public Company? Company { get; private set; }

    [JsonIgnore]
    public Character? Character { get; private set; }

    [JsonIgnore]
    public ItemTemplate? ItemTemplate { get; private set; }

    public void Create(Guid companyId, Guid characterId, Guid itemTemplateId, int quantity, int costPrice, int sellPrice, Guid? serialNumber)
    {
        CompanyId = companyId;
        CharacterId = characterId;
        ItemTemplateId = itemTemplateId;
        Quantity = quantity;
        CostPrice = costPrice;
        SellPrice = sellPrice;
        SerialNumber = serialNumber;
    }
}