package ru.jizapika.javaserver.Services;

import ru.jizapika.javaserver.Dao.KittensDao;
import ru.jizapika.javaserver.Dao.SimpleKittensDao;
import ru.jizapika.javaserver.Objects.Kitten;

import java.util.List;

public class SimpleKittensService implements KittensService {
    private KittensDao kittenDao = new SimpleKittensDao();

    @Override
    public List<Kitten> allKittens() {
        return kittenDao.kittens();
    }

    @Override
    public void create(Kitten kitten) {
        kittenDao.create(kitten);
    }

    @Override
    public Kitten read(int id) {
        return kittenDao.read(id);
    }

    @Override
    public void update(Kitten kitten, int id) {
        kittenDao.update(kitten, id);
    }

    @Override
    public void delete(int id) {
        kittenDao.delete(id);
    }

    @Override
    public void addFriend(int id, int friendId) {
        kittenDao.addFriend(id, friendId);
    }
}
