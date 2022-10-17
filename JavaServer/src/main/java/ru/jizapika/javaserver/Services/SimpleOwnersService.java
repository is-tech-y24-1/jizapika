package ru.jizapika.javaserver.Services;

import ru.jizapika.javaserver.Dao.OwnersDao;
import ru.jizapika.javaserver.Dao.SimpleOwnersDao;
import ru.jizapika.javaserver.Objects.Owner;

import java.util.List;

public class SimpleOwnersService implements OwnersService {
    private OwnersDao ownersDao = new SimpleOwnersDao();

    @Override
    public List<Owner> allOwners() {
        return ownersDao.owners();
    }

    @Override
    public void create(Owner owner) {
        ownersDao.create(owner);
    }

    @Override
    public Owner read(int id) {
        return ownersDao.read(id);
    }

    @Override
    public void update(Owner owner, int id) {
        ownersDao.update(owner, id);
    }

    @Override
    public void delete(int id) {
        ownersDao.delete(id);
    }

    @Override
    public void addKitten(int id, int kittenId) {
        ownersDao.addKitten(id, kittenId);
    }
}
