import Entity from './Entity.js';

export default interface Person extends Entity {
    FirstName: string;
    LastName: string;
    PhotoUrl: string;
    FullName: string;
    Email: string;
}