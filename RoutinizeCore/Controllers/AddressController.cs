using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HelperLibrary.Shared;
using Microsoft.AspNetCore.Mvc;
using RoutinizeCore.Attributes;
using RoutinizeCore.Models;
using RoutinizeCore.Services.Interfaces;
using RoutinizeCore.ViewModels;

namespace RoutinizeCore.Controllers {

    [ApiController]
    [RoutinizeActionFilter]
    [Route("address")]
    public sealed class AddressController : AppController {

        private readonly IAddressService _addressService;

        public AddressController(
            IAddressService addressService
        ) : base(null) {
            _addressService = addressService;
        }

        [HttpPost("add-one")]
        public async Task<JsonResult> AddLocationSingle(Address address) {
            var errors = address.VerifyLocationData();
            if (errors.Length != 0) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });

            var saveResult = await _addressService.SaveNewAddress(address);
            return !saveResult.HasValue || saveResult.Value < 1
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while saving data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = saveResult.Value });
        }

        [HttpPost("add-many")]
        public async Task<JsonResult> AddLocationsMultiple(Address[] addresses) {
            await _addressService.StartTransaction();
            var errors = Array.Empty<string>();
                
            foreach (var address in addresses) {
                errors = address.VerifyLocationData();
                if (errors.Length != 0) {
                    await _addressService.RevertTransaction();
                    return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
                }
                
                var saveResult = await _addressService.SaveNewAddress(address);
                if (saveResult > 0) continue;

                errors[0] = "An issue happened while saving data.";
                break;
            }

            if (errors.Length != 0) {
                await _addressService.RevertTransaction();
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errors });
            }

            await _addressService.CommitTransaction();
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpDelete("remove/{addressId}")]
        public async Task<JsonResult> DeleteLocation([FromRoute] int addressId) {
            var removeResult = await _addressService.RemoveAddress(addressId);
            return !removeResult.HasValue || !removeResult.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while removing data." })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success });
        }

        [HttpGet("get-one/{addressId}")]
        public async Task<JsonResult> GetLocationSingle([FromRoute] int addressId) {
            var address = await _addressService.GetAddressById(addressId);
            return address == null ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." })
                                   : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = address });
        }

        [HttpPost("get-many")]
        public async Task<JsonResult> GetLocationsMultiple(int[] addressIds) {
            var addresses = new List<Address>();
            foreach (var addressId in addressIds) {
                var address = await _addressService.GetAddressById(addressId);
                if (address == null) return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while getting data." });
                
                addresses.Add(address);
            }
            
            return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success, Data = addresses });
        }

        [HttpPut("update")]
        public async Task<JsonResult> UpdateAddress(Address address) {
            var errors = address.VerifyAddressData();
            if (errors.Count != 0) {
                var errorMessages = address.GenerateErrorMessages(errors);
                return new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Data = errorMessages });
            }
        
            var result = await _addressService.UpdateAddress(address);
            return result.HasValue && result.Value
                ? new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Success })
                : new JsonResult(new JsonResponse { Result = SharedEnums.RequestResults.Failed, Message = "An issue happened while updating data." });
        }

        // [HttpPut("update")]
        // public async Task<JsonResult> UpdateLocation(Address address) {
        //     
        // }
    }
}