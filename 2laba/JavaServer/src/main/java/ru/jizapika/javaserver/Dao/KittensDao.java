package ru.jizapika.javaserver.Dao;

import ru.jizapika.javaserver.Objects.Kitten;

import java.util.List;

public interface KittensDao {
    List<Kitten> kittens();
    public void create(Kitten kitten);

    public Kitten read(int id);

    public void update(Kitten kitten, int id);

    public void addFriend(int id, int friendId);
    public void delete(int id);
}
