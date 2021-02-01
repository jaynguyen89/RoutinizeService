using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Services.Interfaces;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [Route("address")]
    public sealed class AddressController {

        private readonly IAddressService _addressService;

        public AddressController(
            IAddressService addressService
        ) {
            _addressService = addressService;
        }
        
        
    }
}