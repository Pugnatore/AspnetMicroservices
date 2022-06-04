using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using AutoMapper;
using EventBus.Messages.Events;
using MassTransit;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<BasketController> _logger;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository repository, ILogger<BasketController> logger, DiscountGrpcService discountGrpcService, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _logger = logger;
            _discountGrpcService = discountGrpcService;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> GetItems(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
        {
            try
            {
                foreach (var item in basket.Items)
                {
                    var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                    item.Price -= coupon.Amount;
                }
                var basketResult = _repository.UpdateBasket(basket);
                return Ok(basketResult.Result);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
            //get existing basket with total price
            //Create basketCheckoutEvent -- set total price on basketCheckout eventMessage
            //send checkout event to rabbitmq
            //remove the basket

            var basket = await _repository.GetBasket(basketCheckout.UserName);

            if (basket is null)
            {
                return BadRequest();
            }

            //Este mapeamento é importante para sabermos para tiparmos o consumer e assim conseguirmos consumir o evento
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice=basket.TotalPrice; //Not necessary
            await _publishEndpoint.Publish(eventMessage);

            await _repository.DeleteBasket(basketCheckout.UserName);

            return Accepted();
        }
    }
}