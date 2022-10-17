package ru.jizapika.javaserver.Controllers;

import org.springframework.web.bind.annotation.*;
import ru.jizapika.javaserver.Objects.Owner;
import ru.jizapika.javaserver.Services.*;

import java.util.List;

@RestController
@RequestMapping("/owners")
public class OwnersController {
    private OwnersService ownersService = new SimpleOwnersService();

    @GetMapping("/getAll")
    public List<Owner> allOwners() {
        return ownersService.allOwners();
    }

    @PostMapping("/create")
    public void createOwner(
            @RequestBody Owner owner) {
        ownersService.create(owner);
    }

    @GetMapping("/read/{id}")
    public Owner getOwner(@PathVariable int id) {
        return ownersService.read(id);
    }

    @PatchMapping("/update/{id}")
    public void updateOwner(
            @PathVariable  int id,
            @RequestBody Owner owner) {
        ownersService.update(owner, id);
    }

    @DeleteMapping("/delete/{id}")
    public void deleteOwner(@PathVariable int id) {
        ownersService.delete(id);
    }

    @PatchMapping("/addKitten")
    public void addKitten(@RequestParam int id, @RequestParam int kittenId) {
        ownersService.addKitten(id, kittenId);
    }
}