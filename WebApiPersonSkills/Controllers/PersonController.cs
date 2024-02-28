using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebApiPersonSkills;
using WebApiPersonSkills.Models;
using WebApiPersonSkills.Models.ViewModel;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PersonsController : ControllerBase
    {
        private readonly PersonSkillsContext _context;
        private readonly ILogger<PersonsController> _logger;

        public PersonsController(PersonSkillsContext context, ILogger<PersonsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Person>> GetPersons()
        {
            _logger.LogInformation("Getting all persons.");
            var persons = _context.People.Include(p => p.Skills).ToList();
            var personsVM = new List<PersonVM>();
            foreach (var person in persons)
            {
                personsVM.Add(new PersonVM(person));
            }
            return Ok(personsVM);
        }

        [HttpGet("{id}")]
        public ActionResult<PersonVM> GetPerson(long id)
        {
            _logger.LogInformation($"Getting person with ID: {id}.");
            var person = _context.People
                .Where(p => p.Id == id )
                .Include(p => p.Skills)
                .FirstOrDefault();

            if (person == null)
            {
                _logger.LogWarning($"Person with ID: {id} not found.");
                return NotFound();
            }
            return Ok(new PersonVM(person));
        }

        [HttpPost]
        public ActionResult<PersonVM> CreatePerson(PersonVM personVM)
        {

            var person = personVM.GetPerson();

            person.Skills = AddSkills(personVM.SkillVMs);

            _context.People.Add(person);
            _context.SaveChanges();
            
            _logger.LogInformation($"Person created with ID: {person.Id}.");
            personVM = new PersonVM(person);
            return CreatedAtAction(nameof(GetPerson), new { id = personVM.Id }, personVM);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePerson(long id, PersonVM updatedPerson)
        {
            _logger.LogInformation($"Updating person with ID: {id}.");

            var existingPerson = _context
                .People
                .Where(p => p.Id == id)
                .Include(p => p.Skills).FirstOrDefault();

            if (existingPerson == null)
            {
                _logger.LogWarning($"Person with ID: {id} not found.");
                return NotFound();
            }

            existingPerson.Name = updatedPerson.Name;
            existingPerson.DisplayName = updatedPerson.DisplayName;
            existingPerson.Skills?.Clear();

            existingPerson.Skills = AddSkills(updatedPerson.SkillVMs);

            // Обновление навыков сотрудника

            _context.SaveChanges();
            _logger.LogInformation($"Person with ID: {id} updated.");
            return Ok();
        }

        private ICollection<Skill> AddSkills(List<SkillVM> skillVMs)
        {
            var result = new List<Skill>();
            foreach (var skillVM in skillVMs)
            {
                var existingSkill = _context
                    .Skills
                    .FirstOrDefault(s => s.Name == skillVM.Name &&
                                              s.Level == skillVM.Level);

                if (existingSkill == null)
                {
                    existingSkill = skillVM.GetSkill();
                    _context.Skills.Add(existingSkill); // Добавляем навык в контекст EF Core
                }
                result.Add(existingSkill);
            }
            return result;
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePerson(long id)
        {
            _logger.LogInformation($"Deleting person with ID: {id}.");
            var person = _context.People.Find(id);
            if (person == null)
            {
                _logger.LogWarning($"Person with ID: {id} not found.");
                return NotFound();
            }

            _context.People.Remove(person);
            _context.SaveChanges();
            _logger.LogInformation($"Person with ID: {id} deleted.");
            return Ok();
        }
    }
}
