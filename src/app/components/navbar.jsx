"use client";

import { useState } from "react";
import Link from "next/link";

const Navbar = () => {
  // Dummy login state
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  // Example user data
  const user = {
    name: "Chamikara",
    image: "https://i.pravatar.cc/40", // random avatar placeholder
  };

  return (
    <nav className="bg-[#1C1C1C] text-[#D4AF37] py-2 px-8 flex justify-between items-center font-serif text-lg">
      
      {/* Logo */}
      <div className="flex-1">
        <Link href="/" className="text-3xl font-bold tracking-wider">
          NumisLive
        </Link>
      </div>

      {/* Navigation Links */}
      <div className="flex-1 flex justify-center space-x-12 font-poppins">
        <Link href="/" className="hover:underline">Home</Link>
        <Link href="/auctions" className="hover:underline">Auctions</Link>
        <Link href="/browse-coins" className="hover:underline">Browse Coins</Link>
        <Link href="/about-us" className="hover:underline">About Us</Link>
      </div>

      {/* Right Side: Login or User Avatar */}
      <div className="flex-1 flex justify-end">
        {!isLoggedIn ? (
          <button
            onClick={() => setIsLoggedIn(true)}
            className="border-2 border-custom-gold rounded-2xl px-6 py-1 transition duration-300 hover:bg-custom-gold hover:text-[#a2872c]"
          >
            Login
          </button>
        ) : (
          <img
            src={user.image}
            alt={user.name}
            className="w-10 h-10 rounded-full border-2 border-custom-gold cursor-pointer"
          />
        )}
      </div>
    </nav>
  );
};

export default Navbar;
