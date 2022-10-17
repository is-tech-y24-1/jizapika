package ru.jizapika.javaserver.Controllers;

import org.springframework.web.bind.annotation.*;
import ru.jizapika.javaserver.Services.KittensService;
import ru.jizapika.javaserver.Objects.Kitten;
import ru.jizapika.javaserver.Services.SimpleKittensService;

import java.util.*;

@RestController
@RequestMapping("/kittens")
public class KittensController {
    private KittensService kittensService = new SimpleKittensService();

    @GetMapping("/getAll")
    public List<Kitten> allKittens() {
        return kittensService.allKittens();
    }

    @PostMapping("/create")
    public void createKitten(
            @RequestBody Kitten kitten) {
        kittensService.create(kitten);
    }

    @GetMapping("/read/{id}")
    public Kitten getKitten(@PathVariable int id) {
        return kittensService.read(id);
    }

    @PatchMapping("/update/{id}")
    public void updateKitten(
            @PathVariable  int id,
            @RequestBody Kitten kitten) {
        kittensService.update(kitten, id);
    }

    @DeleteMapping("/delete/{id}")
    public void deleteKitten(@PathVariable int id) {
        kittensService.delete(id);
    }

    @PatchMapping("/addFriend")
    public void addFriend(@RequestParam int catId, @RequestParam int friendId) {
        kittensService.addFriend(catId, friendId);
    }
}