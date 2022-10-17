package ru.jizapika.javaserver.Dao;

import ru.jizapika.javaserver.Objects.Kitten;

import java.util.ArrayList;
import java.util.List;

public class SimpleKittensDao implements KittensDao {
    List<Kitten> kittens = new ArrayList<>();

    @Override
    public List<Kitten> kittens() {
        return this.kittens;
    }

    @Override
    public void create(Kitten kitten) {
        kittens.add(kitten);
    }

    @Override
    public Kitten read(int id) {
        return kittens.get(id);
    }

    @Override
    public void update(Kitten kitten, int id) {
        kittens.set(id, kitten);
    }

    @Override
    public void addFriend(int id, int friendId) {
        kittens.get(id).addFriend(friendId);
    }

    @Override
    public void delete(int id) {
        kittens.remove(id);
    }
}