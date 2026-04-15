using CharacterBuilderApp.Models;
using Microsoft.AspNetCore.Mvc;
using CharacterBuilderApp.Services;

namespace CharacterBuilderApp.Controllers
{
    public class CharacterController : Controller
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _characterService.GetAllAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CharacterCreateInput characterCreateInput)
        {

            if (!ModelState.IsValid)
            {
                return View(characterCreateInput);
            }
            try
            {
                 await _characterService.CreateAsync(characterCreateInput);
            }
            catch (CharacterPersistenceException)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the character. Please try again.");
                return View(characterCreateInput);
            }

            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var character = await _characterService.GetByIdAsync(id);
            if (character == null)
            {
                return NotFound();
            }
            return View(character);
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var characterEditInput = await _characterService.GetEditInputAsync(id);
            if (characterEditInput == null)
            {
                return NotFound();
            }            
            return View(characterEditInput);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CharacterEditInput characterEditInput)
        {
            if (!ModelState.IsValid)
            {
                return View(characterEditInput);
            }

            var result = await _characterService.UpdateAsync(characterEditInput);


            return result switch
            {
                CharacterUpdateResult.Updated => RedirectToAction("Details", new { id = characterEditInput.Id }),
                CharacterUpdateResult.Unchanged => RedirectToAction("Details", new { id = characterEditInput.Id }),
                CharacterUpdateResult.NotFound => NotFound(),
                CharacterUpdateResult.Failed => Problem("Unable to update character right now."),
                _ => Problem("Unexpected update result.")
            };       
            
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var character = await _characterService.GetByIdAsync(id);
            if (character == null)
            {
                return NotFound();
            }

            return View(character);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _characterService.DeleteAsync(id);

            return result switch
            {
                CharacterDeleteResult.Deleted => RedirectToAction("Index"),
                CharacterDeleteResult.NotFound => NotFound(),
                CharacterDeleteResult.Failed => StatusCode(500, "An error occurred while deleting the character."),
                _ => Problem("Unexpected delete result.")
            };

        }
    }
}