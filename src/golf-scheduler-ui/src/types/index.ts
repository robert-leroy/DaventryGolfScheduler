export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  phone: string | null;
  handicap: number | null;
  isAdmin: boolean;
}

export interface UserProfile extends User {
  createdAt: string;
}

export interface UserProfileUpdate {
  firstName: string;
  lastName: string;
  email: string;
  phone: string | null;
  handicap: number | null;
}

export interface UserCreate {
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  handicap?: number;
  isAdmin?: boolean;
}

export interface UserUpdate {
  email?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  handicap?: number;
  isAdmin?: boolean;
}

export interface TeeTime {
  id: string;
  teeDate: string;
  teeTime: string;
  maxPlayers: number;
  notes: string | null;
  createdAt: string;
  createdBy: User;
  registrations: Registration[];
  availableSlots: number;
}

export interface TeeTimeListItem {
  id: string;
  teeDate: string;
  teeTime: string;
  maxPlayers: number;
  notes: string | null;
  registeredCount: number;
  availableSlots: number;
  isUserRegistered: boolean;
}

export interface TeeTimeCreate {
  teeDate: string;
  teeTime: string;
  maxPlayers: number;
  notes?: string;
}

export interface TeeTimeUpdate {
  teeDate?: string;
  teeTime?: string;
  maxPlayers?: number;
  notes?: string;
}

export interface Registration {
  id: string;
  userId: string;
  userDisplayName: string;
  registeredAt: string;
}

export interface UserRegistration {
  id: string;
  teeTimeId: string;
  teeDate: string;
  teeTime: string;
  registeredAt: string;
}

export interface GolfersByDay {
  date: string;
  dayOfWeek: string;
  teeTimes: TeeTimeWithGolfers[];
}

export interface TeeTimeWithGolfers {
  id: string;
  teeTime: string;
  notes: string | null;
  golfers: string[];
}
