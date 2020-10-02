using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;

namespace MvcMapping.Mappers
{
    public class AutoMapperConfig
    {
        public static void RegisterMapping()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DomainToViewModelMappingProfiles>();
                x.AddProfile<ViewModelToDomainMappingProfiles>();
            });
        }
    }
}
