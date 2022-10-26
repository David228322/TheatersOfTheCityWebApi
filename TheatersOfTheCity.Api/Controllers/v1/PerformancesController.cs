using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TheatersOfTheCity.Api.Controllers.Extensions;
using TheatersOfTheCity.Contracts.v1.Request;
using TheatersOfTheCity.Core.Data;
using System.Linq;
using TheatersOfTheCity.Contracts.Common;
using TheatersOfTheCity.Contracts.v1.Response;
using TheatersOfTheCity.Core.Domain;

namespace TheatersOfTheCity.Api.Controllers.v1
{
    
    public class PerformancesController : BaseEntitiesController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public PerformancesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IEnumerable<PerformanceResponse>),StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var performances = await _unitOfWork.PerformanceRepository.GetAllAsync();
            if (performances == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<IEnumerable<PerformanceResponse>>(performances);
            return Ok(response.ToApiResponse());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PerformanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var performance = await _unitOfWork.PerformanceRepository.GetByIdAsync(id);
            if (performance == null)
            {
                return NotFound();
            }

            var response = _mapper.Map<PerformanceResponse>(performance);
            return Ok(response.ToApiResponse());
        }

        [HttpPost]
        [ProducesResponseType(typeof(PerformanceResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public  async Task<IActionResult> Create(CreatePerformanceRequest request)
        {
            //TODO: Update participant
            var requestScenes = _mapper.Map<IEnumerable<Scene>>(request.Participants);
            
            var newPerformance = _mapper.Map<Performance>(request);
            var performance = await _unitOfWork.PerformanceRepository.CreateAsync(newPerformance);
            var response = _mapper.Map<PerformanceResponse>(performance);
            
            if (request.Participants.Any())
            {
                var contacts = await _unitOfWork.ContactRepository.GetManyByIdAsync(requestScenes.Select(x => x.ParticipantId), nameof(Contact.ContactId));
                if (contacts.Count() != requestScenes.Count())
                {
                    NotFound(contacts.ToApiResponse("Not all contacts are exist"));
                }
                
                requestScenes.ToList().ForEach(x => x.Participant = contacts.First(c => c.ContactId == x.ParticipantId));
                requestScenes.ToList().ForEach(x => x.ParticipantId = x.Participant.ContactId);
                requestScenes.ToList().ForEach(x => x.Performance = new Lookup(){Id = response.PerformanceId, Name = response.Name});
                requestScenes.ToList().ForEach(x => x.PerformanceId = response.PerformanceId);


                var scenes = await _unitOfWork.SceneRepository.CreateManyAsync(requestScenes);
                response.Participants = _mapper.Map<IEnumerable<SceneResponse>>(scenes);
            }
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdatePerformanceRequest request, [FromRoute] int id)
        {
            var updatePerformanceRequest = await _unitOfWork.PerformanceRepository.GetByIdAsync(id);
            if (updatePerformanceRequest == null)
            {
                return NotFound(updatePerformanceRequest.ToApiResponse("Current performance doesn't exist"));
            }

            var mappedPerformance = _mapper.Map(request, updatePerformanceRequest);
            var performance = await _unitOfWork.PerformanceRepository.UpdateAsync(mappedPerformance);

            var response = _mapper.Map<PerformanceResponse>(performance);
            response.Participants.ToList().ForEach(x => x.Performance.Name = response.Name);
            return Ok(response.ToApiResponse());
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteById(int id)
        {
            var performance = await _unitOfWork.PerformanceRepository.GetByIdAsync(id);
            if (performance == null)
            {
                return NotFound();
            }

            await _unitOfWork.PerformanceRepository.DeleteByIdAsync(id);
            
            return NoContent();
        }
    }
}
