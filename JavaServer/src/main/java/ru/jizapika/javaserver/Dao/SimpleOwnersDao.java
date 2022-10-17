package ru.jizapika.javaserver.Dao;

import ru.jizapika.javaserver.Objects.Owner;

import java.util.ArrayList;
import java.util.List;

public class SimpleOwnersDao implements OwnersDao {
    List<Owner> owners = new ArrayList<>();

    @Override
    public List<Owner> owners() {
        return this.owners;
    }

    @Override
    public void create(Owner owner) {
        owners.add(owner);
    }

    @Override
    public Owner read(int id) {
        return owners.get(id);
    }

    @Override
    public void update(Owner owner, int id) {
        owners.set(id, owner);
    }

    @Override
    public void delete(int id) {
        owners.remove(id);
    }

    @Override
    public void addKitten(int id, int kittenId) {
        owners.get(id).addKitten(kittenId);
    }
}
