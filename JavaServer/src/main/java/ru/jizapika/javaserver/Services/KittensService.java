package ru.jizapika.javaserver.Services;

import ru.jizapika.javaserver.Objects.Kitten;

import java.util.List;

public interface KittensService {
    List<Kitten> allKittens();
    public void create(Kitten kitten);
    public Kitten read(int id);
    public void update(Kitten kitten, int id);
    public void delete(int id);

    public void addFriend(int id, int friendId);
}