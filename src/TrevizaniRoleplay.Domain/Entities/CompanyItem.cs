using System.Text.Json.Serialization;

namespace TrevizaniRoleplay.Domain.Entities;

public class CompanyItem : BaseEntity
{
    public Guid CompanyId﻿ { get; private set; }
    public Guid ItemTemplateId { get; private set; }
    public int CostPrice﻿ { get; private set; }
    public int SellPrice﻿ { get; private set; }

    [JsonIgnore]
    public Company? Company { get; private set; }

    [JsonIgnore]
    public ItemTemplate? ItemTemplate { get; private set; }

    public void Create(Guid companyId, Guid itemTemplateId, int costPrice)
    {
        CompanyId = companyId;
        ItemTemplateId = itemTemplateId;
        CostPrice = costPrice;
    }

    public void Update(Guid itemTemplateId, int costPrice)
    {
        ItemTemplateId = itemTemplateId;
        CostPrice = costPrice;
    }

    public void Update(int sellPrice)
    {
        SellPrice﻿ = sellPrice;
    }
}