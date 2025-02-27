using System.Text.Json.Serialization;
using TrevizaniRoleplay.Domain.Enums;

namespace TrevizaniRoleplay.Domain.Entities;

public class CompanyTuningPrice : BaseEntity
{
    public Guid CompanyId﻿ { get; private set; }
    public CompanyTuningPriceType Type { get; private set; }
    public float CostPercentagePrice﻿ { get; private set; }
    public float SellPercentagePrice﻿ { get; private set; }

    [JsonIgnore]
    public Company? Company { get; private set; }

    public void Create(Guid companyId, CompanyTuningPriceType type, float costPercentagePrice﻿)
    {
        CompanyId = companyId;
        Type = type;
        CostPercentagePrice﻿ = costPercentagePrice﻿;
    }

    public void SetCostPercentagePrice﻿(float costPercentagePrice﻿)
    {
        CostPercentagePrice﻿ = costPercentagePrice﻿;
    }

    public void SetSellPercentagePrice(float sellPercentagePrice)
    {
        SellPercentagePrice = sellPercentagePrice;
    }
}